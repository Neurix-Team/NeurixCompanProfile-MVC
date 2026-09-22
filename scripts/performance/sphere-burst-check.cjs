const {chromium}=require('../../.perf-tools/node_modules/playwright-core');
const assert=require('node:assert/strict');
const fs=require('node:fs/promises');

(async()=>{
 const browser=await chromium.launch({executablePath:'C:/Program Files/Google/Chrome/Application/chrome.exe',headless:true});
 try {
  const page=await browser.newPage({viewport:{width:1440,height:900}});
  const errors=[];
  page.on('pageerror',e=>errors.push(e.message));
  await page.addInitScript(()=>localStorage.setItem('neurix-theme','dark'));
  await page.goto('http://127.0.0.1:8080/Home/Labs?lang=ar');
  await page.waitForFunction(()=>document.querySelector('canvas')?.dataset.renderer==='worker');
  await page.screenshot({path:'.perf-tools/burst-top.png'});
  const scroll=async progress=>{
   await page.evaluate(p=>scrollTo({top:(document.documentElement.scrollHeight-innerHeight)*p,behavior:'instant'}),progress);
   await page.waitForTimeout(550);
  };
  await scroll(.81);
  await page.screenshot({path:'.perf-tools/burst-middle.png'});
  await scroll(1);
  await page.screenshot({path:'.perf-tools/burst-bottom.png'});
  const sample=async(withScroll=false)=>page.evaluate(withScroll=>new Promise(resolve=>{
   const frames=[],tasks=[];
   const duration=withScroll?4000:2000;
   const distance=document.documentElement.scrollHeight-innerHeight;
   const observer=new PerformanceObserver(list=>tasks.push(...list.getEntries().map(e=>e.duration)));
   observer.observe({type:'longtask'});
   let start,last;
   function frame(t){
    if(!start)start=t;
    if(withScroll)scrollTo({top:distance*Math.abs(1-2*Math.min(1,(t-start)/duration)),behavior:'instant'});
    if(last)frames.push(t-last);
    last=t;
    if(t-start<duration)requestAnimationFrame(frame);
    else {observer.disconnect();frames.sort((a,b)=>a-b);resolve({p95:frames[Math.floor(frames.length*.95)],max:frames.at(-1),over50:frames.filter(f=>f>50).length,longTasks:tasks})}
   }
   requestAnimationFrame(frame);
  }),withScroll);
  const desktop=await sample();
  const scrolling=await sample(true);
  const isolated=await page.addStyleTag({content:'body > :not(#neurix-gl) { opacity:0 !important; transition:none !important } #neurix-network { display:none !important }'});
  const first=await page.screenshot({path:'.perf-tools/burst-particles-a.png'});
  await page.waitForTimeout(700);
  const second=await page.screenshot({path:'.perf-tools/burst-particles-b.png'});
  assert(!first.equals(second),'Particles must remain animated at the bottom');
  await isolated.evaluate(e=>e.remove());
  await page.evaluate(()=>window.toggleTheme());
  await page.waitForTimeout(350);
  await page.screenshot({path:'.perf-tools/burst-light.png'});
  await page.setViewportSize({width:390,height:844});
  await page.reload();
  await page.waitForFunction(()=>document.querySelector('canvas')?.dataset.renderer==='worker');
  await scroll(1);
  assert(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth));
  await page.screenshot({path:'.perf-tools/burst-mobile.png'});
  const mobile=await sample();
  const mobileScrolling=await sample(true);
  await page.emulateMedia({reducedMotion:'reduce'});
  await page.waitForTimeout(250);
  await page.addStyleTag({content:'body > :not(#neurix-gl) { opacity:0 !important; transition:none !important } #neurix-network { display:none !important }'});
  await page.waitForTimeout(600);
  const reducedFirst=await page.screenshot({path:'.perf-tools/burst-reduced-a.png'});
  await page.waitForTimeout(350);
  assert(reducedFirst.equals(await page.screenshot({path:'.perf-tools/burst-reduced-b.png'})),'Reduced motion must remain still');
  const fallback=await browser.newPage({viewport:{width:1440,height:900}});
  await fallback.addInitScript(()=>window.Worker=undefined);
  fallback.on('pageerror',e=>errors.push(e.message));
  await fallback.goto('http://127.0.0.1:8080/');
  await fallback.waitForFunction(()=>document.querySelector('canvas')?.dataset.renderer==='main');
  await fallback.evaluate(()=>scrollTo({top:document.documentElement.scrollHeight,behavior:'instant'}));
  await fallback.waitForTimeout(500);
  await fallback.screenshot({path:'.perf-tools/burst-fallback.png'});
  assert.deepEqual(errors,[]);
  const result={desktop,scrolling,mobile,mobileScrolling,errors,checks:['worker','particles persist and move at bottom','light and dark','mobile overflow','reduced motion still','main-thread fallback']};
  await fs.writeFile('.perf-tools/burst-results.json',JSON.stringify(result,null,2));
  console.log(JSON.stringify(result));
 } finally {await browser.close()}
})().catch(e=>{console.error(e);process.exit(1)});
