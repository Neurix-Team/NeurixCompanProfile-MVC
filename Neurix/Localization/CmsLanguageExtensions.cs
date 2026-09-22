using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace Neurix.Localization
{
    public static class CmsLanguageExtensions
    {
        /// <summary>Cookie that carries the chosen dashboard language between requests.</summary>
        public static readonly string CookieName = CookieRequestCultureProvider.DefaultCookieName;

        /// <summary>
        /// The current URL (path + query) for the language toggle to return to, with any
        /// existing "lang" parameter removed.
        ///
        /// Stripping it matters: the query-string culture provider outranks the cookie, so
        /// returning to "/cms/services?lang=ar" after switching to English would
        /// immediately re-apply Arabic and the button would look broken.
        /// </summary>
        public static string CurrentUrlForLanguageToggle(this HttpRequest request)
        {
            var kept = request.Query
                .Where(kv => !string.Equals(kv.Key, CmsLanguage.QueryKey, StringComparison.OrdinalIgnoreCase))
                .SelectMany(kv => kv.Value.Select(v => (kv.Key, Value: v)))
                .ToList();

            var path = request.Path.HasValue ? request.Path.Value! : "/";

            if (kept.Count == 0)
            {
                return path;
            }

            var query = string.Join("&", kept.Select(p =>
                $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value ?? string.Empty)}"));

            return $"{path}?{query}";
        }

        public static IServiceCollection AddCmsLocalization(this IServiceCollection services)
        {
            services.AddSingleton<ICmsLocalizer, CmsLocalizer>();

            // Localize [Display(Name = "...")] on every view model. Configured through
            // AddOptions so the singleton localizer can be injected — MvcOptions is not
            // resolvable at AddControllersWithViews() time.
            services.AddOptions<Microsoft.AspNetCore.Mvc.MvcOptions>()
                .Configure<ICmsLocalizer>((options, localizer) =>
                {
                    options.ModelMetadataDetailsProviders.Add(
                        new LocalizedDisplayMetadataProvider(localizer));
                    // Must be registered too, otherwise implicit [Required] and the bare
                    // [StringLength]/[Range] attributes keep the framework's English text.
                    options.ModelMetadataDetailsProviders.Add(
                        new DefaultValidationMessageProvider());
                });

            // Guarantees implicit [Required] and bare [StringLength]/[Range] carry a
            // message the localizer can translate (see the class comment).
            services.AddSingleton<
                Microsoft.AspNetCore.Mvc.DataAnnotations.IValidationAttributeAdapterProvider,
                CmsValidationAttributeAdapterProvider>();

            // Point DataAnnotations at the same JSON dictionaries. Configured here rather
            // than in the AddDataAnnotationsLocalization lambda so the singleton localizer
            // arrives by injection instead of building a throwaway service provider.
            services.AddOptions<Microsoft.AspNetCore.Mvc.DataAnnotations.MvcDataAnnotationsLocalizationOptions>()
                .Configure<ICmsLocalizer>((options, localizer) =>
                    options.DataAnnotationLocalizerProvider =
                        (_, _) => new CmsStringLocalizerAdapter(localizer));

            services.Configure<RequestLocalizationOptions>(options =>
            {
                // Formatting culture is pinned to English on purpose. Switching
                // CurrentCulture to ar-* would change decimal separators and calendars,
                // which would break model binding for fields like Deal.Value and any
                // date input. Only the UI culture follows the user's choice.
                options.SupportedCultures = new List<CultureInfo> { new(CmsLanguage.DefaultCode) };
                options.SupportedUICultures = CmsLanguage.Codes.Select(c => new CultureInfo(c)).ToList();
                options.DefaultRequestCulture = new RequestCulture(
                    culture: CmsLanguage.DefaultCode,
                    uiCulture: CmsLanguage.DefaultCode);

                // Deliberately no Accept-Language provider: the dashboard should default
                // to English until someone explicitly switches, rather than flipping to
                // Arabic because of a browser setting.
                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider
                    {
                        QueryStringKey = CmsLanguage.QueryKey,
                        UIQueryStringKey = CmsLanguage.QueryKey
                    },
                    new CookieRequestCultureProvider()
                };
            });

            return services;
        }

        /// <summary>
        /// Persists ?lang=xx into the culture cookie.
        ///
        /// The query-string provider only affects the request it appears on, so without
        /// this the language would reset on the next navigation. Must run BEFORE
        /// UseRequestLocalization so the value is validated once and written early.
        /// </summary>
        public static IApplicationBuilder UseCmsLanguageCookie(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var requested = context.Request.Query[CmsLanguage.QueryKey].ToString();

                if (!string.IsNullOrWhiteSpace(requested) && CmsLanguage.IsSupported(requested))
                {
                    var language = CmsLanguage.FromCode(requested);
                    context.Response.Cookies.Append(
                        CookieName,
                        CookieRequestCultureProvider.MakeCookieValue(
                            new RequestCulture(CmsLanguage.DefaultCode, language.Code)),
                        new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddYears(1),
                            IsEssential = true,
                            HttpOnly = false,   // a future client-side toggle may read it
                            SameSite = SameSiteMode.Lax,
                            Path = "/"
                        });
                }

                await next();
            });
        }
    }
}
