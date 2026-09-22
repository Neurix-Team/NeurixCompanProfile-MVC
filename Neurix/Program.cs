using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Neurix.BLL.DependencyInjection;
using Neurix.Common;
using Neurix.Localization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// DataAnnotations localization translates every ErrorMessage on the view models via the
// JSON dictionaries; the provider itself is configured in AddCmsLocalization().
builder.Services.AddControllersWithViews()
    .AddDataAnnotationsLocalization();
builder.Services.AddHttpClient();

// Backs the short-lived read caches in the CMS services (company profile, menus, home
// sections, social links) — the same rows get re-queried on almost every request
// (navbar + footer + the page itself all ask for the same company profile), so caching
// them is what makes repeat page loads fast instead of doing it over on every request.
builder.Services.AddMemoryCache();

// The antiforgery token must never be readable from script and must never travel on a
// cross-site request. SameSite=Strict is safe here because the token is only posted
// back to the same page that rendered the form.
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "Neurix.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    // Always in production: the site is served over TLS. SameAsRequest keeps the
    // plain-http dev profile usable without weakening the production setting.
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

// ── CRM: database, identity and business services (see Neurix.BLL) ──
builder.Services.AddCrm(builder.Configuration);

// ── CMS: database and business services (see Neurix.BLL) ──
builder.Services.AddCms(builder.Configuration);

// ── CMS/CRM dashboard localization (infrastructure only — no strings translated yet) ──
// The public site keeps its own client-side data-en/data-ar engine; this is separate.
builder.Services.AddCmsLocalization();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/crm/account/login";
    options.LogoutPath = "/crm/account/logout";
    options.AccessDeniedPath = "/crm/account/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

// ── Sign-in brute-force protection (per IP, independent of Identity lockout) ──
// The policy is attached to the login POST only, so a shared office IP is not
// throttled on normal dashboard traffic. Identity's per-account lockout still
// applies underneath this.
builder.Services.AddRateLimiter(options =>
{
    var perMinutePerIp = (int limit) => (Microsoft.AspNetCore.Http.HttpContext context) =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });

    options.AddPolicy(CrmRateLimitPolicies.Login, perMinutePerIp(5));
    // Looser than login: a real visitor submits the footer form at most once, but a
    // shared office/NAT IP should not get locked out of subscribing entirely.
    options.AddPolicy(CrmRateLimitPolicies.Newsletter, perMinutePerIp(10));

    options.OnRejected = (context, cancellationToken) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";

        // A rejected browser form POST would otherwise render a bare 429 page;
        // send the user back to the login form with a neutral notice instead.
        if (context.HttpContext.Request.Path.StartsWithSegments("/crm/account"))
        {
            context.HttpContext.Response.Redirect("/crm/account/login?throttled=1");
            return ValueTask.CompletedTask;
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        return ValueTask.CompletedTask;
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Friendly pages for body-less error statuses (missing page, missing asset). Scoped to
// GET/HEAD only: ReExecute preserves the original HTTP method, and re-issuing a POST/PUT/etc.
// against a GET-only error-page action makes routing report a misleading 405 instead of the
// real status — every POST-based failure in the app (anti-forgery, validation, rate limits)
// would surface as "Method Not Allowed" instead of its actual code. POST endpoints already
// render their own in-form error state, so they do not need this page at all.
app.UseWhen(
    context => HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method),
    branch => branch.UseStatusCodePagesWithReExecute("/Home/ShowStatusCode", "?code={0}"));

// Baseline response hardening. Runs first so every response — static assets,
// uploads, dashboards — carries the headers. CSP is deliberately Report-Only
// (SecurityHeaders section in appsettings): enforcing it blindly would break
// the public layout's inline scripts; flip it to enforced once a real traffic
// report comes back clean.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers.XContentTypeOptions = "nosniff";
    headers.XFrameOptions = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), geolocation=(), microphone=(self), payment=()";

    var reportOnly = context.RequestServices.GetRequiredService<IConfiguration>()
        ["SecurityHeaders:ContentSecurityPolicyReportOnly"];
    if (!string.IsNullOrWhiteSpace(reportOnly))
    {
        headers["Content-Security-Policy-Report-Only"] = reportOnly;
    }

    await next();
});

app.UseHttpsRedirection();
app.UseWebSockets();

// ── CMS uploads: serve user-uploaded media straight off the filesystem ──
// MapStaticAssets() below only serves the asset manifest generated at BUILD time,
// so files written after the app is published (CMS image uploads) are invisible to
// it and return 404. This middleware maps /uploads to wwwroot/uploads and resolves
// each request against the disk, so newly uploaded media is served immediately.
var uploadsPath = Path.Combine(
    app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"),
    "uploads");
Directory.CreateDirectory(uploadsPath); // PhysicalFileProvider throws if the folder is missing

app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/uploads",
    FileProvider = new PhysicalFileProvider(uploadsPath)
});

// ── Dashboard language: persist ?lang=xx, then resolve the request's UI culture ──
// Order matters: the cookie is written first, then RequestLocalization reads the
// query string (this request) or the cookie (subsequent requests).
app.UseCmsLanguageCookie();
app.UseRequestLocalization();

app.UseRouting();

// Reject throttled login attempts before they reach Identity's own lockout check.
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// ── CRM/CMS: migrate + seed, in the background ──
// Both initializers already catch and log every failure internally (see
// CrmDatabaseInitializer/CmsDatabaseInitializer) and every read elsewhere in the app is
// fail-soft, so nothing needs this to finish before the server starts accepting requests.
// Awaiting it here used to block startup on the database round-trip (migrations against a
// remote/cold database can take several seconds), which is exactly what made a restart feel
// slow — the app was up but silent until this finished. It now starts serving immediately;
// the first request or two may see fallback content until migration/seeding completes.
_ = Task.Run(async () =>
{
    await app.Services.InitializeCrmDatabaseAsync();
    await app.Services.InitializeCmsDatabaseAsync();
});

app.Run();

/// <summary>Exposes the top-level-statement entry point to WebApplicationFactory&lt;Program&gt; in Neurix.Tests.</summary>
public partial class Program { }
