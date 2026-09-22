using System;
using System.Globalization;

namespace Neurix.Common
{
    /// <summary>
    /// Converts CMS brand colours (stored as hex, e.g. "#00C2D4") into the
    /// space-separated HSL triplets used by the Neurix design system.
    ///
    /// neurix.css declares its palette as bare triplets (e.g. --azure: 211 100% 50%)
    /// so Tailwind can compose them with an alpha channel: hsl(var(--azure) / 0.25).
    /// A raw hex value cannot be substituted into those variables, which is why the
    /// CMS colours have to be translated before they are injected into the page.
    /// </summary>
    public static class BrandColorHelper
    {
        /// <summary>
        /// Converts "#RRGGBB" (also "#RGB" and "#RRGGBBAA") to "H S% L%".
        /// Returns null when the value is missing or not a valid hex colour, so
        /// callers can fall back to the stylesheet default.
        /// </summary>
        public static string? ToHslTriplet(string? hex, double lightnessScale = 1.0)
        {
            if (!TryParseHex(hex, out var r, out var g, out var b))
            {
                return null;
            }

            RgbToHsl(r, g, b, out var h, out var s, out var l);

            if (lightnessScale != 1.0)
            {
                l = Math.Clamp(l * lightnessScale, 0d, 100d);
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} {1}% {2}%",
                Math.Round(h),
                Math.Round(s),
                Math.Round(l));
        }

        /// <summary>
        /// True when the string is a hex colour we can safely emit into a stylesheet.
        /// Used as a guard so untrusted CMS input can never break out of the CSS rule.
        /// </summary>
        public static bool IsValidHex(string? hex) => TryParseHex(hex, out _, out _, out _);

        private static bool TryParseHex(string? hex, out int r, out int g, out int b)
        {
            r = g = b = 0;

            if (string.IsNullOrWhiteSpace(hex))
            {
                return false;
            }

            var value = hex.Trim().TrimStart('#');

            // #RGB shorthand -> expand each nibble ("0af" => "00aaff")
            if (value.Length == 3)
            {
                value = string.Concat(value[0], value[0], value[1], value[1], value[2], value[2]);
            }

            // Accept #RRGGBBAA but ignore the alpha channel; CSS handles opacity separately.
            if (value.Length == 8)
            {
                value = value.Substring(0, 6);
            }

            if (value.Length != 6)
            {
                return false;
            }

            foreach (var c in value)
            {
                if (!Uri.IsHexDigit(c))
                {
                    return false;
                }
            }

            r = int.Parse(value.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            g = int.Parse(value.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            b = int.Parse(value.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return true;
        }

        private static void RgbToHsl(int r255, int g255, int b255, out double h, out double s, out double l)
        {
            var r = r255 / 255d;
            var g = g255 / 255d;
            var b = b255 / 255d;

            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));
            var delta = max - min;

            l = (max + min) / 2d;

            if (delta == 0d)
            {
                h = 0d;
                s = 0d;
            }
            else
            {
                s = delta / (1d - Math.Abs(2d * l - 1d));

                if (max == r)
                {
                    h = 60d * (((g - b) / delta) % 6d);
                }
                else if (max == g)
                {
                    h = 60d * (((b - r) / delta) + 2d);
                }
                else
                {
                    h = 60d * (((r - g) / delta) + 4d);
                }

                if (h < 0d)
                {
                    h += 360d;
                }
            }

            s *= 100d;
            l *= 100d;
        }
    }
}
