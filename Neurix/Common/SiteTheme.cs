using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Neurix.BLL.Dtos.Cms;

namespace Neurix.Common
{
    /// <summary>A brand colour family: drives buttons, links, gradients, glows and the hero sphere.</summary>
    public sealed record ThemePalette(
        string Key, string NameEn, string NameAr,
        string DeepBlue, string Azure, string Cyan, string Navy);

    /// <summary>
    /// A surface set for one mode: page background, cards, borders and text.
    /// Card/Secondary/Popover may be null on the dark default, meaning "follow the palette's navy"
    /// (that is how neurix.css defines them).
    /// </summary>
    public sealed record ThemeShade(
        string Key, string NameEn, string NameAr,
        string Background, string Foreground, string? Card, string? Secondary,
        string Muted, string MutedForeground, string Border, string? Glow = null);

    /// <summary>The three choices an admin makes on the CMS Theme page.</summary>
    public sealed record SiteThemeSelection(ThemePalette Palette, ThemeShade Light, ThemeShade Dark)
    {
        public bool IsDefaultPalette => Palette.Key == SiteTheme.DefaultPaletteKey;
        public bool IsDefaultLight => Light.Key == SiteTheme.DefaultLightKey;
        public bool IsDefaultDark => Dark.Key == SiteTheme.DefaultDarkKey;
        public bool IsAllDefault => IsDefaultPalette && IsDefaultLight && IsDefaultDark;
    }

    /// <summary>
    /// Preset catalogue for the CMS Theme page and the CSS injected into every layout.
    ///
    /// Values are bare HSL triplets, matching neurix.css ("211 100% 50%") so Tailwind's
    /// hsl(var(--x) / alpha) keeps working. Only values from this catalogue are ever written
    /// into a &lt;style&gt; — the stored setting is just a preset key, never raw CSS.
    /// </summary>
    public static class SiteTheme
    {
        public const string SettingGroup = "Theme";
        public const string KeyPrefix = "theme.";
        public const string PaletteKey = "theme.palette";
        public const string LightKey = "theme.light";
        public const string DarkKey = "theme.dark";

        public const string DefaultPaletteKey = "neurix";
        public const string DefaultLightKey = "snow";
        public const string DefaultDarkKey = "black";

        public static readonly IReadOnlyList<ThemePalette> Palettes = new[]
        {
            // Default: identical to neurix.css, so nothing is emitted for it.
            new ThemePalette("neurix",  "Neurix Blue",   "أزرق نيوركس",   "217 100% 39%", "211 100% 50%", "211 100% 58%", "218 57% 17%"),
            new ThemePalette("ocean",   "Ocean Teal",    "أزرق محيطي",    "192 100% 28%", "189 94% 38%",  "186 85% 46%",  "200 60% 15%"),
            new ThemePalette("emerald", "Emerald",       "زمردي",         "161 84% 24%",  "158 72% 36%",  "155 62% 45%",  "165 50% 13%"),
            new ThemePalette("violet",  "Violet",        "بنفسجي",        "263 70% 42%",  "262 78% 57%",  "258 90% 68%",  "256 45% 17%"),
            new ThemePalette("indigo",  "Royal Indigo",  "نيلي ملكي",     "234 70% 40%",  "234 82% 57%",  "230 90% 67%",  "232 50% 17%"),
            new ThemePalette("sunset",  "Sunset Orange", "برتقالي غروب",  "17 88% 40%",   "22 92% 48%",   "28 94% 50%",   "20 45% 14%"),
            new ThemePalette("rose",    "Rose",          "وردي",          "340 75% 38%",  "342 78% 53%",  "346 85% 63%",  "340 40% 15%"),
            new ThemePalette("crimson", "Crimson",       "قرمزي",         "354 78% 36%",  "355 74% 48%",  "0 84% 60%",    "355 40% 14%"),
            new ThemePalette("gold",    "Royal Gold",    "ذهبي",          "32 90% 30%",   "36 92% 38%",   "40 90% 44%",   "35 45% 13%"),
            new ThemePalette("graphite","Graphite",      "رمادي جرافيت",  "220 16% 26%",  "218 14% 40%",  "214 16% 56%",  "220 22% 13%"),
        };

        public static readonly IReadOnlyList<ThemeShade> LightShades = new[]
        {
            // Default: identical to neurix.css :root.
            new ThemeShade("snow",  "Snow (default)", "ثلجي (افتراضي)", "210 40% 98%", "218 57% 17%", "0 0% 100%",  "210 40% 96%", "210 40% 96%", "215 16% 40%", "214 32% 91%"),
            new ThemeShade("white", "Pure White",     "أبيض ناصع",      "0 0% 100%",   "222 47% 11%", "0 0% 100%",  "220 14% 96%", "220 14% 96%", "220 9% 40%",  "220 13% 90%"),
            new ThemeShade("mist",  "Cool Mist",      "ضبابي بارد",     "214 36% 94%", "218 57% 17%", "210 40% 99%","214 32% 90%", "214 32% 90%", "215 18% 36%", "214 26% 84%"),
            new ThemeShade("ivory", "Warm Ivory",     "عاجي دافئ",      "40 33% 97%",  "25 25% 15%",  "40 40% 99%", "38 26% 93%",  "38 26% 93%",  "30 10% 38%",  "36 20% 86%"),
            new ThemeShade("pearl", "Pearl Gray",     "رمادي لؤلؤي",    "0 0% 95%",    "0 0% 12%",    "0 0% 99%",   "0 0% 91%",    "0 0% 91%",    "0 0% 36%",    "0 0% 85%"),
            new ThemeShade("mint",  "Soft Mint",      "نعناعي هادئ",    "150 25% 96%", "160 30% 12%", "0 0% 100%",  "150 18% 91%", "150 18% 91%", "160 8% 36%",  "150 14% 85%"),
        };

        public static readonly IReadOnlyList<ThemeShade> DarkShades = new[]
        {
            // Default: identical to neurix.css .dark (cards follow the palette's navy).
            new ThemeShade("black",    "Pure Black (default)", "أسود خالص (افتراضي)", "0 0% 0%",     "0 0% 100%",   null,           null,           "218 40% 12%", "215 20% 75%", "218 40% 18%"),
            new ThemeShade("midnight", "Midnight Navy",        "كحلي منتصف الليل",    "222 47% 7%",  "210 40% 98%", "220 45% 12%",  "220 40% 15%",  "221 42% 10%", "215 20% 72%", "218 35% 20%", "218 60% 30%"),
            new ThemeShade("slate",    "Deep Slate",           "أردوازي داكن",        "222 25% 11%", "210 30% 97%", "220 22% 16%",  "220 20% 19%",  "221 22% 13%", "215 15% 72%", "218 18% 25%", "218 30% 35%"),
            new ThemeShade("charcoal", "Charcoal",             "فحمي",                "0 0% 7%",     "0 0% 98%",    "0 0% 11%",     "0 0% 14%",     "0 0% 10%",    "0 0% 70%",    "0 0% 20%",    "0 0% 40%"),
            new ThemeShade("dim",      "Dim Gray",             "رمادي خافت",          "215 15% 15%", "210 20% 93%", "215 14% 20%",  "215 13% 24%",  "215 14% 17%", "215 12% 70%", "215 12% 29%", "215 20% 40%"),
            new ThemeShade("plum",     "Deep Plum",            "برقوقي داكن",         "265 30% 7%",  "270 30% 97%", "265 25% 12%",  "265 22% 15%",  "265 25% 10%", "265 12% 72%", "265 20% 21%", "265 40% 32%"),
        };

        public static ThemePalette FindPalette(string? key) =>
            Palettes.FirstOrDefault(p => string.Equals(p.Key, key?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? Palettes[0];

        public static ThemeShade FindLight(string? key) =>
            LightShades.FirstOrDefault(s => string.Equals(s.Key, key?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? LightShades[0];

        public static ThemeShade FindDark(string? key) =>
            DarkShades.FirstOrDefault(s => string.Equals(s.Key, key?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? DarkShades[0];

        public static bool IsKnownPalette(string? key) => Palettes.Any(p => p.Key == key);
        public static bool IsKnownLight(string? key) => LightShades.Any(s => s.Key == key);
        public static bool IsKnownDark(string? key) => DarkShades.Any(s => s.Key == key);

        public static SiteThemeSelection FromSettings(IEnumerable<CmsSiteSettingDto>? settings)
        {
            string? Value(string key) => settings?.FirstOrDefault(s => s.Key == key)?.ValueEn;
            return new SiteThemeSelection(FindPalette(Value(PaletteKey)), FindLight(Value(LightKey)), FindDark(Value(DarkKey)));
        }

        /// <summary>
        /// Builds the override stylesheet. Must be emitted AFTER neurix.css.
        ///
        /// &lt;html&gt; matches both :root and .dark with equal specificity, so light surfaces go under
        /// :root:not(.dark) — a plain :root rule placed after neurix.css would beat its .dark block
        /// and leak light surfaces into dark mode.
        /// </summary>
        public static string BuildCss(SiteThemeSelection theme)
        {
            var css = new StringBuilder();

            if (!theme.IsDefaultPalette)
            {
                var p = theme.Palette;
                css.Append(":root,.dark{")
                   .Append("--deep-blue:").Append(p.DeepBlue).Append(';')
                   .Append("--azure:").Append(p.Azure).Append(';')
                   .Append("--cyan:").Append(p.Cyan).Append(';')
                   .Append("--navy:").Append(p.Navy).Append(';')
                   .Append("--primary:").Append(p.Azure).Append(';')
                   .Append("--accent:").Append(p.Cyan).Append(';')
                   .Append('}');
                css.Append(":root:not(.dark){--ring:").Append(p.Azure).Append(";}");
                css.Append(".dark{--ring:").Append(p.Cyan).Append(";}");
            }

            if (!theme.IsDefaultLight)
            {
                css.Append(":root:not(.dark){");
                AppendSurfaces(css, theme.Light);
                css.Append('}');
            }

            if (!theme.IsDefaultDark)
            {
                css.Append(".dark{");
                AppendSurfaces(css, theme.Dark);
                if (theme.Dark.Glow != null)
                {
                    css.Append("--gradient-hero:radial-gradient(ellipse 80% 60% at 50% 40%, hsl(")
                       .Append(theme.Dark.Glow).Append(" / 0.24), transparent 76%);");
                }
                css.Append('}');
            }

            return css.ToString();
        }

        private static void AppendSurfaces(StringBuilder css, ThemeShade s)
        {
            var card = s.Card ?? "var(--navy)";
            var secondary = s.Secondary ?? "var(--navy)";

            css.Append("--background:").Append(s.Background).Append(';')
               .Append("--foreground:").Append(s.Foreground).Append(';')
               .Append("--card:").Append(card).Append(';')
               .Append("--card-foreground:").Append(s.Foreground).Append(';')
               .Append("--popover:").Append(card).Append(';')
               .Append("--popover-foreground:").Append(s.Foreground).Append(';')
               .Append("--secondary:").Append(secondary).Append(';')
               .Append("--secondary-foreground:").Append(s.Foreground).Append(';')
               .Append("--muted:").Append(s.Muted).Append(';')
               .Append("--muted-foreground:").Append(s.MutedForeground).Append(';')
               .Append("--border:").Append(s.Border).Append(';')
               .Append("--input:").Append(s.Border).Append(';');
        }

        /// <summary>
        /// WebGL colours (0–1 RGB) for the hero sphere, or null for the default palette so the
        /// renderer keeps its hand-tuned constants. Order: [lightDeep, lightBright, darkDeep, darkBright].
        /// </summary>
        public static string? BuildSphereJson(SiteThemeSelection theme)
        {
            if (theme.IsDefaultPalette)
            {
                return null;
            }

            var p = theme.Palette;
            var colours = new[]
            {
                HslToRgb(p.DeepBlue, 0.85),
                HslToRgb(p.Azure, 0.86),
                HslToRgb(p.Azure, 1.0),
                HslToRgb(p.Cyan, 1.15),
            };

            return "[" + string.Join(",", colours.Select(c => "[" + string.Join(",", c.Select(v => v.ToString("0.###", CultureInfo.InvariantCulture))) + "]")) + "]";
        }

        /// <summary>Hex for swatches in the CMS ("211 100% 50%" → "#0076ff").</summary>
        public static string ToHex(string? triplet)
        {
            if (triplet == null)
            {
                return "transparent";
            }

            var rgb = HslToRgb(triplet, 1.0);
            return "#" + string.Concat(rgb.Select(v => ((int)Math.Round(v * 255)).ToString("x2", CultureInfo.InvariantCulture)));
        }

        private static double[] HslToRgb(string triplet, double lightnessScale)
        {
            var parts = triplet.Replace("%", string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var h = double.Parse(parts[0], CultureInfo.InvariantCulture);
            var s = double.Parse(parts[1], CultureInfo.InvariantCulture) / 100d;
            var l = Math.Clamp(double.Parse(parts[2], CultureInfo.InvariantCulture) / 100d * lightnessScale, 0d, 1d);

            var c = (1d - Math.Abs(2d * l - 1d)) * s;
            var x = c * (1d - Math.Abs(h / 60d % 2d - 1d));
            var m = l - c / 2d;

            var (r, g, b) = h switch
            {
                < 60 => (c, x, 0d),
                < 120 => (x, c, 0d),
                < 180 => (0d, c, x),
                < 240 => (0d, x, c),
                < 300 => (x, 0d, c),
                _ => (c, 0d, x),
            };

            return new[] { r + m, g + m, b + m };
        }
    }
}
