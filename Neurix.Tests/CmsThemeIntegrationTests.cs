using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Neurix.BLL.Common;
using Neurix.DAL.Data;
using Neurix.DAL.Models;
using Neurix.Tests.Infrastructure;

namespace Neurix.Tests;

public sealed class CmsThemeIntegrationTests : IDisposable
{
    private const string Password = "Str0ng!Passw0rd";
    private readonly NeurixWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public CmsThemeIntegrationTests()
    {
        _factory.EnsureDatabasesCreated();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CmsDbContext>();
            db.CompanyProfiles.Add(new CmsCompanyProfile
            {
                Id = Guid.NewGuid(),
                Slug = "neurix",
                NameEn = "Neurix AI",
                NameAr = "نيوركس",
                AccentColor = "#5B5FEF",
                IsPublished = true
            });
            db.SaveChanges();
        }
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task SavedTheme_IsAppliedToPublicSiteAndHiddenFromGenericSettings()
    {
        await LoginAsAdminAsync();

        var page = await _client.GetAsync("/cms/settings/theme");
        page.EnsureSuccessStatusCode();
        var pageHtml = await page.Content.ReadAsStringAsync();
        Assert.Contains("name=\"Palette\" value=\"emerald\"", pageHtml);

        var home = await (await _client.GetAsync("/")).Content.ReadAsStringAsync();
        Assert.DoesNotContain("id=\"cms-site-theme\"", home);
        Assert.Contains("id=\"cms-brand-colors\"", home);

        var saved = await _client.PostAsync("/cms/settings/theme", Form(await GetTokenAsync("/cms/settings/theme"), "emerald", "ivory", "charcoal"));
        Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);

        home = await (await _client.GetAsync("/")).Content.ReadAsStringAsync();
        Assert.Contains("id=\"cms-site-theme\"", home);
        Assert.Contains("--azure:158 72% 36%", home);
        Assert.Contains(":root:not(.dark){--background:40 33% 97%", home);
        Assert.Contains(".dark{--background:0 0% 7%", home);
        Assert.Contains("window.neurixSphereColors", home);
        // A chosen palette replaces the profile's accent override.
        Assert.DoesNotContain("id=\"cms-brand-colors\"", home);

        var settings = await (await _client.GetAsync("/cms/settings")).Content.ReadAsStringAsync();
        Assert.DoesNotContain("theme.palette", settings);
    }

    [Fact]
    public async Task UnknownPresetKey_IsRejected()
    {
        await LoginAsAdminAsync();

        var saved = await _client.PostAsync("/cms/settings/theme", Form(await GetTokenAsync("/cms/settings/theme"), "red;}body{display:none", "ivory", "charcoal"));
        Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);

        var home = await (await _client.GetAsync("/")).Content.ReadAsStringAsync();
        Assert.DoesNotContain("id=\"cms-site-theme\"", home);
    }

    private static FormUrlEncodedContent Form(string token, string palette, string light, string dark) => new(new Dictionary<string, string>
    {
        ["Palette"] = palette,
        ["Light"] = light,
        ["Dark"] = dark,
        ["__RequestVerificationToken"] = token
    });

    private async Task LoginAsAdminAsync()
    {
        const string email = "theme-admin@example.test";
        using (var scope = _factory.Services.CreateScope())
        {
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.True((await roles.CreateAsync(new ApplicationRole { Name = CrmRoles.Admin })).Succeeded);
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = "Theme Admin" };
            Assert.True((await users.CreateAsync(user, Password)).Succeeded);
            Assert.True((await users.AddToRoleAsync(user, CrmRoles.Admin)).Succeeded);
        }

        var token = await GetTokenAsync("/crm/account/login");
        var login = await _client.PostAsync("/crm/account/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = Password,
            ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
    }

    private async Task<string> GetTokenAsync(string path)
    {
        var response = await _client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(token);
        return token;
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
