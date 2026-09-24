using Neurix.BLL.Dtos.Cms;
using Neurix.Common;
using Xunit;

namespace Neurix.Tests;

public class SiteThemeTests
{
    private static CmsSiteSettingDto Setting(string key, string value) => new() { Key = key, ValueEn = value };

    [Fact]
    public void BuildCss_AllDefaults_EmitsNothing()
    {
        var theme = SiteTheme.FromSettings(null);

        Assert.True(theme.IsAllDefault);
        Assert.Equal(string.Empty, SiteTheme.BuildCss(theme));
        Assert.Null(SiteTheme.BuildSphereJson(theme));
    }

    [Fact]
    public void BuildCss_ScopesLightShadeAwayFromDarkMode()
    {
        var theme = SiteTheme.FromSettings(new[]
        {
            Setting(SiteTheme.PaletteKey, "emerald"),
            Setting(SiteTheme.LightKey, "ivory"),
            Setting(SiteTheme.DarkKey, "charcoal"),
        });

        var css = SiteTheme.BuildCss(theme);

        // A plain :root rule emitted after neurix.css would beat its .dark block.
        Assert.Contains(":root:not(.dark){--background:40 33% 97%;", css);
        Assert.Contains(".dark{--background:0 0% 7%;", css);
        Assert.Contains(":root,.dark{--deep-blue:161 84% 24%;", css);
        Assert.NotNull(SiteTheme.BuildSphereJson(theme));
    }

    [Fact]
    public void FromSettings_UnknownOrMaliciousKey_FallsBackToDefault()
    {
        var theme = SiteTheme.FromSettings(new[]
        {
            Setting(SiteTheme.PaletteKey, "}</style><script>alert(1)</script>"),
            Setting(SiteTheme.DarkKey, "nope"),
        });

        Assert.True(theme.IsAllDefault);
        Assert.Equal(string.Empty, SiteTheme.BuildCss(theme));
    }

    [Fact]
    public void ToHex_ConvertsDesignSystemTriplet()
    {
        Assert.Equal("#ff0000", SiteTheme.ToHex("0 100% 50%"));
        Assert.Equal("#007bff", SiteTheme.ToHex("211 100% 50%"));
    }
}
