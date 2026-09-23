using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos.Cms;
using Neurix.BLL.Services.Cms;
using Neurix.DAL.Data;
using Neurix.DAL.Models;
using Neurix.Tests.Infrastructure;

namespace Neurix.Tests;

public sealed class CmsPublicPropagationIntegrationTests : IDisposable
{
    private const string Password = "Str0ng!Passw0rd";
    private readonly Guid _profileId = Guid.NewGuid();
    private readonly NeurixWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public CmsPublicPropagationIntegrationTests()
    {
        _factory.EnsureDatabasesCreated();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CmsDbContext>();
            db.CompanyProfiles.Add(new CmsCompanyProfile
            {
                Id = _profileId,
                Slug = "neurix",
                NameEn = "Original Brand",
                NameAr = "الاسم الأصلي",
                Email = "original@example.test",
                IsPublished = true
            });
            db.SaveChanges();
        }
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task AdminEditsHeroAndProfile_AppearOnCachedPublicPagesImmediately()
    {
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/Home/About")).StatusCode);
        await LoginAsAdminAsync();

        var heroToken = await GetTokenAsync($"/cms/home-sections/hero?companyId={_profileId}");
        var heroSave = await _client.PostAsync("/cms/home-sections/hero", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["CompanyProfileId"] = _profileId.ToString(),
            ["BadgeEn"] = "Updated public hero badge",
            ["BadgeAr"] = "شارة رئيسية جديدة",
            ["IsPublished"] = "true",
            ["__RequestVerificationToken"] = heroToken
        }));
        Assert.Equal(HttpStatusCode.Redirect, heroSave.StatusCode);

        var home = WebUtility.HtmlDecode(await (await _client.GetAsync("/")).Content.ReadAsStringAsync());
        Assert.Contains("Updated public hero badge", home);
        Assert.Contains("شارة رئيسية جديدة", home);

        var profileToken = await GetTokenAsync($"/cms/companies/{_profileId}/edit");
        var profileSave = await _client.PostAsync($"/cms/companies/{_profileId}/edit", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["NameEn"] = "Updated Public Brand",
            ["NameAr"] = "العلامة الجديدة",
            ["Slug"] = "neurix",
            ["Email"] = "updated@example.test",
            ["IsPublished"] = "true",
            ["__RequestVerificationToken"] = profileToken
        }));
        Assert.Equal(HttpStatusCode.Redirect, profileSave.StatusCode);

        var about = WebUtility.HtmlDecode(await (await _client.GetAsync("/Home/About")).Content.ReadAsStringAsync());
        Assert.Contains("Updated Public Brand", about);
        Assert.Contains("العلامة الجديدة", about);
        Assert.Contains("mailto:updated@example.test", about);

        using (var scope = _factory.Services.CreateScope())
        {
            var profiles = scope.ServiceProvider.GetRequiredService<ICmsCompanyProfileService>();
            Assert.True((await profiles.SetPublishedStatusAsync(_profileId, false)).Success);
        }

        var unpublishedHome = WebUtility.HtmlDecode(await (await _client.GetAsync("/")).Content.ReadAsStringAsync());
        Assert.DoesNotContain("Updated public hero badge", unpublishedHome);
        Assert.DoesNotContain("Updated Public Brand", unpublishedHome);
    }

    [Fact]
    public async Task MenuAndSocialChanges_AppearAfterPublicCacheWasWarmed()
    {
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/")).StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var menus = scope.ServiceProvider.GetRequiredService<ICmsMenuItemService>();
            var socials = scope.ServiceProvider.GetRequiredService<ICmsSocialLinkService>();
            Assert.True((await menus.CreateMenuItemAsync(new CmsMenuItemUpsertDto
            {
                CompanyProfileId = _profileId,
                LabelEn = "Fresh Navigation Link",
                LabelAr = "رابط جديد",
                Url = "/Home/About",
                Placement = CmsMenuItemPlacement.Header,
                IsPublished = true
            })).Success);
            Assert.True((await socials.CreateAsync(new CmsSocialLinkUpsertDto
            {
                CompanyProfileId = _profileId,
                Platform = "linkedin",
                Url = "https://example.test/new-social",
                DisplayName = "New Social Link",
                IsPublished = true
            })).Success);
        }

        var home = WebUtility.HtmlDecode(await (await _client.GetAsync("/")).Content.ReadAsStringAsync());
        Assert.Contains("Fresh Navigation Link", home);
        Assert.Contains("رابط جديد", home);
        Assert.Contains("https://example.test/new-social", home);
    }

    private async Task LoginAsAdminAsync()
    {
        const string email = "cms-admin@example.test";
        using (var scope = _factory.Services.CreateScope())
        {
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.True((await roles.CreateAsync(new ApplicationRole { Name = CrmRoles.Admin })).Succeeded);
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = "CMS Admin" };
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
