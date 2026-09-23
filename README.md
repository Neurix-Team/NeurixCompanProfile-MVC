# Neurix Company Profile — MVC, CMS and CRM

A bilingual Arabic/English ASP.NET Core 10 MVC application with a public company website, a content management dashboard, and a customer relationship management dashboard.

**Deployment status:** verified locally; the included Docker Compose configuration is for development. Read [the production readiness assessment](PRODUCTION_READINESS_AR.md) before exposing the application publicly.

## Project structure

| Path | Responsibility |
|---|---|
| `Neurix/` | MVC controllers, Razor views, localization, middleware, static assets |
| `Neurix.BLL/` | Business services, DTOs, validation, dependency injection |
| `Neurix.DAL/` | EF Core entities, Identity, DbContexts, migrations |
| `Neurix.Tests/` | xUnit service, controller and HTTP integration tests |
| `scripts/` | Image optimization, sphere preview generation and browser performance checks |

The web layer calls BLL services, and the BLL uses EF Core through the DAL. CRM and CMS have separate DbContexts and migration history tables within the configured SQL Server database.

## Features

- Arabic and English public pages with RTL support.
- CMS editing for company profiles, pages, sections, media, navigation, services, team members, testimonials, projects and articles.
- CRM companies, contacts, leads, deals, lead conversion and profile management.
- Contact enquiries captured as CRM leads and newsletter subscriptions.
- Text and voice AI widget using an optional Gemini API key.
- Blue particle sphere with scroll scaling, dispersion and persistent background particles. Rendering uses OffscreenCanvas in a Worker where supported, with a main-thread fallback and reduced-motion support.
- Card reveal and hover animation, responsive navigation, local fonts and libraries, and optimized WebP images.

## Editing public content

Sign in as an Admin at `/crm`, then use **Edit Website Sections** or **Switch to CMS**. Changes to published content appear on the public site after saving; English and Arabic are edited separately.

| Public content | CMS location |
| --- | --- |
| Homepage headings, descriptions, cards and calls to action | Homepage Sections |
| Division page hero titles, subtitles and mission | Divisional Pages |
| Other division sections; AI, Insights, Portfolio and About section headings/cards | Page Sections |
| About, Contact and legal page heroes/body | Content Pages |
| Navbar links and footer company links | Menu Items |
| Navbar contact button, footer headings and division links, office addresses, copyright, Contact form labels, Coming Soon and 404 copy | Global Site Settings |
| Brand name, logo, tagline and primary contact details | Brand Profiles & Colors |
| Service/project/article/team/testimonial card content | The corresponding CMS collection |

The CMS dashboard also has direct links to these editors. Shared text settings are seeded by key on startup without replacing values already edited by an administrator. Public reads of shared settings use a short cache keyed by the CMS revision, so a saved edit takes effect on the next request without an extra database query on every page view. Validation errors and system-generated status messages remain application UI strings.

## Run locally with Docker

Prerequisites: Git and Docker with Compose support.

```powershell
git clone https://github.com/Neurix-Team/NeurixCompanProfile-MVC.git
cd NeurixCompanProfile-MVC
Copy-Item .env.example .env
```

Edit `.env` and choose your own SQL Server and initial administrator passwords, then start the stack:

```powershell
docker compose up -d --build
```

| Surface | Local URL |
|---|---|
| Public website | http://localhost:18473 |
| Sign-in | http://localhost:18473/crm/account/login |
| CMS | http://localhost:18473/cms |
| CRM | http://localhost:18473/crm |

The initial administrator is configured through `SEED_ADMIN_EMAIL` and `SEED_ADMIN_PASSWORD`. Seed credentials create a missing account; changing these variables does not reset an existing user's password. Keep `.env` out of Git.

Compose starts SQL Server and the web application. Migrations and seeding run in the background; a running HTTP listener does not prove database initialization has completed. Inspect startup logs when diagnosing dashboard issues.

```powershell
docker compose logs --tail 100 web
docker compose down
```

Named volumes retain database data, CMS uploads and Data Protection keys across container recreation. `docker compose down -v` deletes those volumes and their data.

## Configuration

| Local variable | Purpose |
|---|---|
| `NEURIX_WEB_PORT` | Host HTTP port; default 18473. Older `WEB_PORT` entries are ignored. |
| `NEURIX_PROXY_HOSTNAME` | Hostname in the web service's `neurix.proxy.domain` label; default `company.neurix.uk`. Older `NEURIX_PROXY_DOMAIN` entries are ignored. |
| `NEURIX_TRUSTED_PROXY_IP` | IP address of the TLS reverse proxy as seen by the web container; enables trusted forwarded HTTP scheme/IP headers |
| `SQLSERVER_PORT` | Host SQL Server port; default 14330 |
| `MSSQL_IMAGE_TAG`, `MSSQL_PID` | SQL Server image and edition; defaults are for development |
| `MSSQL_SA_PASSWORD`, `DATABASE_NAME` | Local database credentials and database name |
| `ASPNETCORE_ENVIRONMENT` | Compose defaults to Development |
| `SEED_ADMIN_EMAIL`, `SEED_ADMIN_PASSWORD`, `SEED_ADMIN_FULLNAME` | Initial administrator |
| `SEED_ADMIN_ALLOW_IN_PRODUCTION` | Explicit opt-in for initial admin creation outside Development; default false |
| `GEMINI_API_KEY` | Optional AI integration; leave empty to disable |

Outside Compose, configure `ConnectionStrings__CrmDb`, `Gemini__ApiKey` and `Crm__SeedAdmin__*` through environment variables or a secret store. Tracked appsettings files contain no live credentials.

The application uses ASP.NET Core Identity cookies. CMS routes require Admin; CRM routes generally allow Admin or CrmStaff. The public AI endpoints are anonymous when enabled and need production abuse controls.

The `web` service advertises `neurix.proxy.domain=company.neurix.uk` and `neurix.proxy.port=18473` by default. The port label follows the published host port (`NEURIX_WEB_PORT`), matching the Neurix proxy convention; SQL Server has no proxy labels. Existing server `.env` files with `WEB_PORT=8080` or `NEURIX_PROXY_DOMAIN=neurix.uk` do not override the new settings. For a different hostname, set `NEURIX_PROXY_HOSTNAME` in `.env` to the exact DNS name without `https://` or a path. The proxy must be able to reach the published port, and DNS must point the name to that proxy. These labels provide routing metadata; the proxy performs certificate issuance and TLS termination. For HTTPS deployment, set `ASPNETCORE_ENVIRONMENT=Production` and `NEURIX_TRUSTED_PROXY_IP` to the proxy's source IP as seen by the web container, and have the proxy send `X-Forwarded-Proto`. Only the configured proxy IP is trusted for forwarded headers. Restrict direct access to the web and SQL Server host ports to the intended proxy and administrators. Production admin seeding is disabled by default; use a unique password and explicitly opt in only for initial account creation.

## Development and validation

Install the .NET 10 SDK. Node.js is needed only for rebuilding Tailwind CSS and running the optional browser checks; prebuilt browser assets are included.

```powershell
dotnet restore Neurix.slnx
dotnet test Neurix.Tests/Neurix.Tests.csproj -c Release
npm --prefix Neurix ci
npm --prefix Neurix run build:css
```

For memory-constrained machines:

```powershell
$env:DOTNET_PROCESSOR_COUNT = '2'
dotnet test Neurix.Tests/Neurix.Tests.csproj -c Release --no-restore --disable-build-servers -m:1 /p:UseSharedCompilation=false /p:BuildInParallel=false /p:ConcurrentBuild=false
```

Validated on 23 September 2026: **265 tests passed**, zero failed or skipped. HTTP-level tests cover dashboard roles, CMS edits appearing on public pages after the cache is warmed, contact enquiries reaching CRM leads, and profile changes. These tests use isolated in-memory databases; they do not replace a staging check against SQL Server. NuGet's vulnerability listing and the npm package-lock audit reported no known vulnerable dependencies in their respective scopes on 22 September 2026. These checks are not a complete security audit or a production load test.

GitHub Actions now runs the build, tests and browser checks on pull requests and pushes to `main`. The browser checks cover Arabic/English layouts, mobile navigation, card motion, and an emulated mobile performance sample. A manual run can inspect an existing staging URL. See [browser quality checks](scripts/quality/README.md) for scope, commands and limitations.

Image optimization requires Pillow: `python scripts/optimize-site-images.py`. Sphere preview regeneration uses the Python standard library: `python scripts/generate-sphere-preview.py`.

## Documentation

- [Production readiness assessment and required deployment work — Arabic](PRODUCTION_READINESS_AR.md)
- [QA implementation guide — Arabic](NEURIX_QA_IMPLEMENTATION_GUIDE_AR.md)
- [Browser performance methodology, results and screenshots](scripts/performance/README.md)
- [Automated browser and staging quality checks](scripts/quality/README.md)

## Production deployment

The repository includes automated quality checks, but not an automated deployment workflow. Pushing to GitHub does not deploy the website; the manual staging check needs an already deployed staging URL.

Before production: configure HTTPS and trusted proxy headers, use a licensed SQL Server edition, isolate the database, use least-privilege credentials, rotate previously exposed keys, disable unintended administrator seeding, add readiness checks, and test backups, restoration, authorization and expected traffic on staging. The complete acceptance list and observed limitations are in [PRODUCTION_READINESS_AR.md](PRODUCTION_READINESS_AR.md).

A legacy Web Deploy profile exists under `Neurix/Properties/PublishProfiles/`; it is not a verified production deployment configuration.
