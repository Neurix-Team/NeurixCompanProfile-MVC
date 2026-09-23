const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const fs = require('node:fs/promises');
const path = require('node:path');

const baseUrl = (process.env.SITE_URL || 'http://127.0.0.1:5099').replace(/\/$/, '');
const outputDir = path.resolve(process.env.QUALITY_OUTPUT || '.quality-results');

async function main() {
    await fs.mkdir(outputDir, { recursive: true });
    const launchOptions = { headless: true };
    if (process.env.CHROME_PATH) launchOptions.executablePath = process.env.CHROME_PATH;
    const browser = await chromium.launch(launchOptions);
    const rows = [];
    const errors = [];

    try {
        const context = await browser.newContext({
            viewport: { width: 390, height: 844 },
            deviceScaleFactor: 2,
            isMobile: true,
            hasTouch: true,
            colorScheme: 'light',
            reducedMotion: 'no-preference'
        });
        await context.addInitScript(() => {
            try { localStorage.setItem('neurix-theme', 'light'); } catch (_) { }
            window.__perf = { lcp: 0, cls: 0, eventDurations: [], longTasks: [], frames: [] };
            try {
                new PerformanceObserver(list => {
                    for (const entry of list.getEntries()) window.__perf.lcp = entry.startTime;
                }).observe({ type: 'largest-contentful-paint', buffered: true });
                new PerformanceObserver(list => {
                    for (const entry of list.getEntries()) if (!entry.hadRecentInput) window.__perf.cls += entry.value;
                }).observe({ type: 'layout-shift', buffered: true });
                new PerformanceObserver(list => {
                    for (const entry of list.getEntries()) if (entry.interactionId) window.__perf.eventDurations.push(entry.duration);
                }).observe({ type: 'event', buffered: true, durationThreshold: 16 });
                new PerformanceObserver(list => {
                    for (const entry of list.getEntries()) window.__perf.longTasks.push(entry.duration);
                }).observe({ type: 'longtask', buffered: true });
            } catch (_) { }
            let last = 0;
            function frame(now) {
                if (last) window.__perf.frames.push(now - last);
                last = now;
                requestAnimationFrame(frame);
            }
            requestAnimationFrame(frame);
        });
        const page = await context.newPage();
        page.on('pageerror', error => errors.push(error.message));
        const cdp = await context.newCDPSession(page);
        await cdp.send('Emulation.setCPUThrottlingRate', { rate: 4 });

        async function record(route) {
            await page.waitForTimeout(1200);
            const row = await page.evaluate(() => {
                const navigation = performance.getEntriesByType('navigation')[0];
                const data = window.__perf;
                const frames = data.frames.slice(5).sort((a, b) => a - b);
                return {
                    route: location.pathname,
                    domContentLoadedMs: Math.round(navigation?.domContentLoadedEventEnd || 0),
                    loadMs: Math.round(navigation?.loadEventEnd || 0),
                    lcpMs: Math.round(data.lcp),
                    cls: Number(data.cls.toFixed(4)),
                    eventDurationMaxMs: Math.round(Math.max(0, ...data.eventDurations)),
                    frameP95Ms: Math.round(frames[Math.floor(frames.length * 0.95)] || 0),
                    framesOver50Ms: frames.filter(value => value > 50).length,
                    longTasks: data.longTasks.length,
                    renderer: document.querySelector('#neurix-sphere-canvas')?.dataset.renderer || 'preview'
                };
            });
            rows.push(row);
            if (row.route !== route) errors.push(`Expected ${route}, got ${row.route}`);
        }

        const first = await page.goto(`${baseUrl}/?lang=en`, { waitUntil: 'load' });
        if (first.status() !== 200) errors.push(`Home status ${first.status()}`);
        await page.locator('#mobile-menu-btn').click();
        await record('/');

        await page.locator('#mobile-menu a[href="/Home/Labs"]').first().click();
        await page.waitForLoadState('load');
        await page.locator('#mobile-menu-btn').click();
        await record('/Home/Labs');

        await page.locator('#mobile-menu a[href="/Home/Contact"]').first().click();
        await page.waitForLoadState('load');
        await record('/Home/Contact');
        await context.close();
    } finally {
        await browser.close();
        await fs.writeFile(path.join(outputDir, 'performance-results.json'), JSON.stringify({ baseUrl, rows, errors }, null, 2));
    }
    console.log(JSON.stringify(rows, null, 2));
    if (errors.length) {
        console.error(errors.join('\n'));
        process.exitCode = 1;
    }
}

main().catch(error => { console.error(error); process.exitCode = 1; });
