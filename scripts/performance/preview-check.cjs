const {chromium}=require('../../.perf-tools/node_modules/playwright-core');
const assert=require('node:assert/strict');
(async()=>{
 const browser=await chromium.launch({executablePath:'C:/Program Files/Google/Chrome/Application/chrome.exe',headless:true});
 const page=await browser.newPage({viewport:{width:1440,height:900}});
 const errors=[];page.on('pageerror',e=>errors.push(e.message));
 await page.route('**/js/neurix-sphere.js*',async route=>{
   await new Promise(r=>setTimeout(r,1000));
   await route.continue();
 });
 for(const reload of [false,true]){
  if(reload) await page.reload({waitUntil:'commit'});
  else await page.goto('http://127.0.0.1:8080/',{waitUntil:'commit'});
  await page.waitForFunction(()=>{const p=document.getElementById('neurix-sphere-preview');return p?.complete&&p.naturalWidth>0&&!p.hidden});
  assert.equal(await page.locator('canvas').getAttribute('data-renderer'),null);
  await page.screenshot({path:`.perf-tools/preview-${reload?'reload':'first'}.png`});
  await page.waitForFunction(()=>document.querySelector('canvas')?.dataset.renderer==='worker');
  assert.equal(await page.locator('#neurix-sphere-preview').evaluate(p=>p.hidden),true);
  console.log(reload?'reload':'first load','preview visible before WebGL, hidden after first frame');
 }
 const resources=await page.evaluate(()=>performance.getEntriesByType('resource').filter(r=>r.name.includes('sphere-preview')).map(r=>({name:r.name,duration:r.duration,bytes:r.transferSize})));
 assert.equal(resources.length,1);console.log('no duplicate image request',resources);
 assert.deepEqual(errors,[]);
 await page.screenshot({path:'.perf-tools/animated.png'});
 await browser.close();
})().catch(e=>{console.error(e);process.exit(1)});
