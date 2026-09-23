const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const fs = require('node:fs/promises');
const path = require('node:path');

const baseUrl = (process.env.SITE_URL || 'http://127.0.0.1:5099').replace(/\/$/, '');
const outputDir = path.resolve(process.env.QUALITY_OUTPUT || '.quality-results');
const full = process.env.QUALITY_SCOPE === 'full';
const defaultRoutes = full
    ? ['/', '/Home/Labs', '/Home/Technology', '/Home/Club', '/Home/Plus', '/Home/HQ', '/Home/Portfolio', '/Home/Insights', '/Home/About', '/Home/Contact', '/Home/AI', '/Home/Privacy', '/Home/Terms', '/Home/Security', '/Home/ComingSoon']
    : ['/', '/Home/Labs', '/Home/Contact'];
const defaultWidths = full ? [280, 320, 360, 390, 768, 1024, 1280, 1440, 1920] : [360, 768, 1440];
const routes = process.env.QUALITY_ROUTES?.split(',').filter(Boolean) || defaultRoutes;
const widths = process.env.QUALITY_WIDTHS?.split(',').map(Number).filter(Boolean) || defaultWidths;
const languages = process.env.QUALITY_LANGS?.split(',').filter(Boolean) || ['en', 'ar'];

async function main() {
    await fs.mkdir(outputDir, { recursive: true });
    const launchOptions = { headless: true };
    if (process.env.CHROME_PATH) launchOptions.executablePath = process.env.CHROME_PATH;
    const browser = await chromium.launch(launchOptions);
    const rows = [];
    const errors = [];

    try {
        for (const lang of languages) {
            const context = await browser.newContext({
                viewport: { width: 1440, height: 900 },
                colorScheme: 'light',
                reducedMotion: 'reduce',
                serviceWorkers: 'block'
            });
            await context.addInitScript(() => {
                try { localStorage.setItem('neurix-theme', 'light'); } catch (_) { }
                window.__qualityVitals = { lcp: 0, cls: 0, shifts: [] };
                try {
                    new PerformanceObserver(list => {
                        for (const entry of list.getEntries()) window.__qualityVitals.lcp = entry.startTime;
                    }).observe({ type: 'largest-contentful-paint', buffered: true });
                    new PerformanceObserver(list => {
                        for (const entry of list.getEntries()) {
                            if (!entry.hadRecentInput) {
                                window.__qualityVitals.cls += entry.value;
                                if (entry.value > 0.01) window.__qualityVitals.shifts.push({
                                    value: Number(entry.value.toFixed(4)),
                                    sources: (entry.sources || []).map(source => ({
                                        element: source.node?.tagName,
                                        className: String(source.node?.className || '').slice(0, 100),
                                        previous: source.previousRect,
                                        current: source.currentRect
                                    }))
                                });
                            }
                        }
                    }).observe({ type: 'layout-shift', buffered: true });
                } catch (_) { }
            });
            for (const width of widths) {
                const page = await context.newPage();
                page.on('pageerror', error => errors.push(`JavaScript: ${error.message}`));
                await page.setViewportSize({ width, height: width < 600 ? 800 : 900 });
                for (const route of routes) {
                    const url = `${baseUrl}${route}${route.includes('?') ? '&' : '?'}lang=${lang}`;
                    let response;
                    try {
                        response = await page.goto(url, { waitUntil: 'load', timeout: 30000 });
                        await page.locator('main').first().waitFor({ timeout: 10000 });
                        await page.waitForTimeout(200);
                    } catch (error) {
                        errors.push(`${lang} ${width} ${route}: ${error.message}`);
                        continue;
                    }
                    const row = await page.evaluate(() => {
                        const nav = document.querySelector('#main-navbar');
                        const main = document.querySelector('#public-main');
                        const values = window.__qualityVitals || {};
                        const timing = performance.getEntriesByType('navigation')[0];
                        const overflow = Math.max(document.documentElement.scrollWidth, document.body.scrollWidth) - innerWidth;
                        const brokenImages = [...document.querySelectorAll('#public-main img')]
                            .filter(img => img.complete && img.naturalWidth === 0)
                            .map(img => img.getAttribute('src'));
                        return {
                            dir: document.documentElement.dir,
                            overflow,
                            mainWidth: Math.round(main?.getBoundingClientRect().width || 0),
                            navWidth: Math.round(nav?.getBoundingClientRect().width || 0),
                            navOverflow: Math.max(0, (nav?.querySelector('nav')?.scrollWidth || 0) - (nav?.querySelector('nav')?.clientWidth || 0)),
                            navVisible: !!nav && getComputedStyle(nav).visibility !== 'hidden',
                            brokenImages,
                            domContentLoadedMs: Math.round(timing?.domContentLoadedEventEnd || 0),
                            lcpMs: Math.round(values.lcp || 0),
                            cls: Number((values.cls || 0).toFixed(4)),
                            shifts: values.shifts || []
                        };
                    });
                    Object.assign(row, { lang, width, route, status: response?.status() });
                    rows.push(row);
                    if (process.env.QUALITY_CAPTURE === '1') {
                        await page.screenshot({ path: path.join(outputDir, `${lang}-${width}-${route.replace(/\W+/g, '-') || 'home'}.png`), fullPage: false });
                    }
                    if (row.status !== 200 || row.overflow > 1 || row.navOverflow > 1 || !row.navVisible || row.mainWidth < width - 2 || row.dir !== (lang === 'ar' ? 'rtl' : 'ltr') || row.brokenImages.length) {
                        errors.push(`${lang} ${width} ${route}: ${JSON.stringify(row)}`);
                        await page.screenshot({ path: path.join(outputDir, `${lang}-${width}-${route.replace(/\W+/g, '-') || 'home'}.png`), fullPage: false });
                    }
                }
                await page.close();
            }
            const page = await context.newPage();
            await page.setViewportSize({ width: 390, height: 844 });
            await page.goto(`${baseUrl}/?lang=${lang}`, { waitUntil: 'load' });
            await page.locator('#mobile-menu-btn').click();
            await page.waitForTimeout(350);
            if (!await page.locator('#mobile-menu').isVisible()) errors.push(`${lang}: mobile menu did not open`);
            await page.locator('#mobile-menu-btn').click();
            await page.waitForTimeout(350);
            if (await page.locator('#mobile-menu').isVisible()) errors.push(`${lang}: mobile menu did not close`);
            await page.evaluate(() => window.toggleTheme());
            if (!await page.locator('html').evaluate(element => element.classList.contains('dark'))) errors.push(`${lang}: theme toggle did not apply`);
            await page.goto(`${baseUrl}/?lang=${lang}`, { waitUntil: 'load' });
            const editorialImage = page.locator('#human-vision img[loading="lazy"]').first();
            await editorialImage.scrollIntoViewIfNeeded();
            try { await editorialImage.evaluate(image => image.decode()); }
            catch (error) { errors.push(`${lang}: first lazy image did not load: ${error.message}`); }
            await context.close();
        }
        const motion = await browser.newContext({ viewport: { width: 1440, height: 900 }, reducedMotion: 'no-preference' });
        const motionPage = await motion.newPage();
        await motionPage.goto(`${baseUrl}/?lang=en`, { waitUntil: 'load' });
        const card = motionPage.locator('main .magnetic-card').first();
        await card.scrollIntoViewIfNeeded();
        await motionPage.waitForTimeout(800);
        if (await card.evaluate(element => element.classList.contains('neurix-reveal-pending'))) errors.push('Card reveal did not finish after scroll');
        const beforeHover = await card.evaluate(element => {
            const style = getComputedStyle(element);
            return { transform: style.transform, shadow: style.boxShadow };
        });
        await card.hover();
        await motionPage.waitForTimeout(350);
        const hover = await card.evaluate(element => {
            const style = getComputedStyle(element);
            return { transform: style.transform, shadow: style.boxShadow };
        });
        if (hover.transform === beforeHover.transform || hover.shadow === beforeHover.shadow) errors.push(`Card hover missing: ${JSON.stringify({ beforeHover, hover })}`);
        await motion.close();
        const access = await browser.newContext();
        for (const route of ['/cms', '/cms/companies', '/crm', '/crm/leads']) {
            const response = await access.request.get(`${baseUrl}${route}`, { maxRedirects: 0 });
            const location = response.headers().location || '';
            if (response.status() !== 302 || !location.toLowerCase().includes('/crm/account/login')) {
                errors.push(`Anonymous access ${route}: ${response.status()} ${location}`);
            }
        }
        await access.close();
    } finally {
        await browser.close();
        await fs.writeFile(path.join(outputDir, 'browser-results.json'), JSON.stringify({ baseUrl, rows, errors }, null, 2));
    }
    console.log(`Checked ${rows.length} page/viewport/language combinations; ${errors.length} errors.`);
    if (errors.length) {
        console.error(errors.slice(0, 30).join('\n'));
        process.exitCode = 1;
    }
}

main().catch(error => { console.error(error); process.exitCode = 1; });
