# Neurix quality checks

The GitHub Actions workflow builds Tailwind CSS, runs the .NET tests, publishes the app, and checks public pages in Chromium at mobile, tablet, and desktop widths. The browser check visits pages in Arabic and English, checks page status, direction, viewport overflow, images, JavaScript errors, the mobile menu, the theme switch, and anonymous redirects for CMS/CRM.

The automatic pull-request check uses three representative pages at 360, 768, and 1440 pixels. Run the full sweep from **Actions → Build and browser quality → Run workflow** with an existing `staging_url`; it covers fifteen public routes at 280, 320, 360, 390, 768, 1024, 1280, 1440, and 1920 pixels in both languages. Dynamic article/project details depend on CMS content and need staging fixtures. This workflow inspects staging; it does not create or deploy a staging server. The .NET integration tests verify Admin and CrmStaff dashboard access with isolated test databases; a staging run still needs a separate check of real accounts and their data permissions.

To run locally, start the site, then install the browser dependency and run:

```powershell
$env:SITE_URL = 'http://127.0.0.1:5099'
$env:QUALITY_SCOPE = 'full'
npm install --prefix scripts/quality
npx --prefix scripts/quality playwright install chromium
npm run check --prefix scripts/quality
npm run performance --prefix scripts/quality
```

`QUALITY_ROUTES`, `QUALITY_WIDTHS`, and `QUALITY_LANGS` accept comma-separated overrides. Results and failure screenshots go to `.quality-results/`, which is ignored by Git. On Windows, `CHROME_PATH` may point to an existing Chrome executable. Browser dependency installation needs npm registry access.

The performance script uses a 390-pixel mobile viewport with four-times CPU throttling and follows the mobile menu from Home to Labs to Contact. It records local LCP, CLS, interaction event duration, long tasks, and frame intervals. These are browser diagnostics, not field Core Web Vitals; event duration is not the INP score. Validate LCP, INP, and CLS from real visits to a deployed site before claiming a mobile performance target; compare results by route and device class. Browser emulation is not a physical-phone test.
