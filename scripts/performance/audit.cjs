const { chromium } = require('../../.perf-tools/node_modules/playwright-core');
const fs = require('node:fs/promises');
(async () => {
 const browser = await chromium.launch({executablePath:'C:/Program Files/Google/Chrome/Application/chrome.exe',headless:true});
 const page = await browser.newPage({viewport:{width:1440,height:900},colorScheme:'dark'});
 const cdp=await page.context().newCDPSession(page);
 if(process.env.CPU) await cdp.send('Emulation.setCPUThrottlingRate',{rate:Number(process.env.CPU)});
 const errors=[];
 page.on('pageerror',e=>errors.push(e.message));
 await page.addInitScript(()=>{
   localStorage.setItem('neurix-theme','dark');
   window.audit={tasks:[],frames:[],gl:[]};
   new PerformanceObserver(l=>audit.tasks.push(...l.getEntries().map(e=>({at:e.startTime,ms:e.duration})))).observe({type:'longtask',buffered:true});
   let last=0;
   function frame(t){if(last)audit.frames.push(t-last);last=t;requestAnimationFrame(frame)}
   requestAnimationFrame(frame);
   for(const [p,methods] of [[HTMLCanvasElement.prototype,['getContext']],[WebGLRenderingContext.prototype,['getShaderParameter','getProgramParameter','getUniformLocation']]]) {
     for(const name of methods){const original=p[name];p[name]=function(...args){const t=performance.now();const result=original.apply(this,args);const ms=performance.now()-t;if(ms>2)audit.gl.push({name,ms});return result;}}
   }
 });
 const rows=[];
 await page.goto('http://127.0.0.1:8080/',{waitUntil:'load'});
 const links=await page.locator('header a[href]').evaluateAll(es=>[...new Set(es.map(e=>e.getAttribute('href')).filter(h=>h==='/'||h.startsWith('/Home/')))]);
 const order=process.env.SHORT?['/Home/About','/','/Home/Labs']:links.concat([...links].reverse());
 console.log('ROUTES',order);
 for(const route of order){
   const target=page.locator(`header a[href="${route}"]:visible`).first();
   await Promise.all([page.waitForNavigation({waitUntil:'load'}),target.click()]);
   await page.waitForTimeout(350);
   await page.mouse.move(700,500);
   for(let i=0;i<4;i++){await page.mouse.wheel(0,650);await page.waitForTimeout(150);}
   for(let i=0;i<4;i++){await page.mouse.wheel(0,-650);await page.waitForTimeout(150);}
   const row=await page.evaluate(()=>{
    const n=performance.getEntriesByType('navigation')[0],p=performance.getEntriesByType('paint').find(e=>e.name==='first-contentful-paint');
    const frames=audit.frames.slice(5).sort((a,b)=>a-b);
    return {path:location.pathname,renderer:document.querySelector('canvas')?.dataset.renderer,dom:Math.round(n.domContentLoadedEventEnd),fcp:p?.startTime,frameP95:Math.round(frames[Math.floor(frames.length*.95)]||0),over50:frames.filter(f=>f>50).length,maxFrame:Math.round(frames.at(-1)||0),longTasks:audit.tasks,gl:audit.gl};
   }); rows.push(row);console.log(JSON.stringify(row));
 }
 await page.screenshot({path:`.perf-tools/${process.env.LABEL||'baseline'}.png`});
 await fs.writeFile(`.perf-tools/${process.env.LABEL||'baseline'}.json`,JSON.stringify({rows,errors},null,2));
 await browser.close();
 if(errors.length) throw new Error(errors.join('\n'));
})().catch(e=>{console.error(e);process.exit(1)});
