using System.Collections.Generic;

namespace Neurix.BLL.Common
{
    /// <summary>
    /// The secondary public pages whose bands are CMS-editable through <c>CmsPageBand</c> and
    /// <c>CmsPageBandItem</c>, and the band keys valid on each. Every band a visitor can see on
    /// these pages is listed here; a key absent from <see cref="BandsFor"/> is rejected by the
    /// service, so a typo cannot quietly create an orphan row that nothing renders.
    /// </summary>
    public static class CmsPageKeys
    {
        public const string Labs = "labs";
        public const string Technology = "technology";
        public const string Hq = "hq";
        public const string Plus = "plus";
        public const string Club = "club";
        public const string Ai = "ai";
        public const string Insights = "insights";
        public const string Portfolio = "portfolio";
        public const string About = "about";

        public static readonly string[] All =
        {
            Labs, Technology, Hq, Plus, Club, Ai, Insights, Portfolio, About
        };

        /// <summary>
        /// The five pages that also have a <c>CmsDivisionPage</c> row for their hero title,
        /// hero subtitle and mission statement.
        /// </summary>
        public static readonly string[] DivisionPages = { Labs, Technology, Hq, Plus, Club };

        /// <summary>True when <paramref name="pageKey"/> is one of the five division pages.</summary>
        public static bool IsDivisionPage(string? pageKey)
        {
            var key = Normalise(pageKey);
            foreach (var div in DivisionPages)
            {
                if (div == key) return true;
            }
            return false;
        }

        /// <summary>Band keys. These are per-page, so the same word can appear on several pages.</summary>
        public static class Bands
        {
            /// <summary>The top band: eyebrow badge plus, on list pages, the whole headline.</summary>
            public const string Hero = "hero";

            /// <summary>The two-column band under the hero: badge, split headline, body, image.</summary>
            public const string Overview = "overview";

            /// <summary>The card grid that closes a division page.</summary>
            public const string Cards = "cards";

            /// <summary>The closing call-to-action band (Technology only).</summary>
            public const string Cta = "cta";

            /// <summary>About: the "What We Stand For" principle cards.</summary>
            public const string Principles = "principles";

            /// <summary>About: the "Five pillars. One vision." heading over the divisions grid.</summary>
            public const string Structure = "structure";

            /// <summary>About: the heading over the leadership grid.</summary>
            public const string Team = "team";
        }

        private static readonly Dictionary<string, string[]> PageBands = new()
        {
            [Labs] = new[] { Bands.Hero, Bands.Overview, Bands.Cards },
            [Technology] = new[] { Bands.Hero, Bands.Overview, Bands.Cta },
            [Hq] = new[] { Bands.Hero, Bands.Overview, Bands.Cards },
            [Plus] = new[] { Bands.Hero, Bands.Overview, Bands.Cards },
            [Club] = new[] { Bands.Hero, Bands.Overview, Bands.Cards },
            [Ai] = new[] { Bands.Hero },
            [Insights] = new[] { Bands.Hero },
            [Portfolio] = new[] { Bands.Hero },
            [About] = new[] { Bands.Principles, Bands.Structure, Bands.Team }
        };

        /// <summary>Band keys valid on <paramref name="pageKey"/>, or empty for an unknown page.</summary>
        public static IReadOnlyList<string> BandsFor(string? pageKey)
        {
            var key = Normalise(pageKey);
            return PageBands.TryGetValue(key, out var bands) ? bands : System.Array.Empty<string>();
        }

        /// <summary>True when <paramref name="pageKey"/> is a known page and the band exists on it.</summary>
        public static bool IsKnown(string? pageKey, string? bandKey)
        {
            var band = Normalise(bandKey);
            foreach (var candidate in BandsFor(pageKey))
            {
                if (candidate == band)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Keys are compared lowercase and trimmed so a stray space or capital from a form post
        /// still finds the row it meant.
        /// </summary>
        public static string Normalise(string? key) =>
            (key ?? string.Empty).Trim().ToLowerInvariant();
    }
}
