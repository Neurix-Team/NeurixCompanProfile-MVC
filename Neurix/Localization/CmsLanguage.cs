using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Neurix.Localization
{
    /// <summary>
    /// The set of languages the dashboard supports, plus the direction metadata the
    /// layout needs. Single source of truth: adding a language here and dropping a
    /// matching cms.&lt;code&gt;.json into Resources/Cms is all that a new locale requires.
    /// </summary>
    public sealed record CmsLanguage(string Code, string NativeName, bool IsRightToLeft)
    {
        public string Dir => IsRightToLeft ? "rtl" : "ltr";

        public static readonly CmsLanguage English = new("en", "English", false);
        public static readonly CmsLanguage Arabic = new("ar", "العربية", true);

        public static readonly IReadOnlyList<CmsLanguage> All = new[] { English, Arabic };

        public const string DefaultCode = "en";

        /// <summary>Query-string key that switches language, e.g. ?lang=ar.</summary>
        public const string QueryKey = "lang";

        public static IReadOnlyList<string> Codes => All.Select(l => l.Code).ToList();

        public static bool IsSupported(string? code) =>
            code is not null && All.Any(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));

        public static CmsLanguage FromCode(string? code) =>
            All.FirstOrDefault(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase)) ?? English;

        /// <summary>
        /// The language for the current request. Reads UI culture only — request
        /// localization is configured so CurrentCulture (number/date formatting) always
        /// stays English, which keeps decimal and date model binding unchanged.
        /// </summary>
        public static CmsLanguage Current =>
            FromCode(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

        /// <summary>
        /// The language the header toggle should switch to: the next one in <see cref="All"/>,
        /// wrapping around. With the current two languages this is simply "the other one";
        /// if a third is added the button cycles and a dropdown would be worth considering.
        /// </summary>
        public static CmsLanguage Next(CmsLanguage current)
        {
            var index = All.ToList().FindIndex(l => l.Code == current.Code);
            return index < 0 ? English : All[(index + 1) % All.Count];
        }
    }
}
