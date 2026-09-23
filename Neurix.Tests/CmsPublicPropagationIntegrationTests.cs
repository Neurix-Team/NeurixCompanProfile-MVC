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

    [Fact]
    public async Task ContentPageAndContactSettingEdits_ReachPublicCopyMetadataAndLinks()
    {
        Guid pageId;
        using (var scope = _factory.Services.CreateScope())
        {
            var pages = scope.ServiceProvider.GetRequiredService<ICmsContentPageService>();
            var created = await pages.UpsertAsync(new CmsContentPageUpsertDto
            {
                CompanyProfileId = _profileId,
                Slug = "about",
                TitleEn = "About Original",
                TitleAr = "عن الشركة",
                IsPublished = true
            });
            Assert.True(created.Success);
            pageId = created.Value;
        }

        await LoginAsAdminAsync();
        var pageToken = await GetTokenAsync($"/cms/content-pages/{pageId}/edit");
        var pageSave = await _client.PostAsync($"/cms/content-pages/{pageId}/edit", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = pageId.ToString(),
            ["CompanyProfileId"] = _profileId.ToString(),
            ["Slug"] = "about",
            ["TitleEn"] = "About Updated",
            ["TitleAr"] = "عن نيوركس الجديدة",
            ["MetaDescriptionEn"] = "Updated English description",
            ["MetaDescriptionAr"] = "وصف عربي جديد",
            ["CtaButtonTextEn"] = "Meet our team",
            ["CtaButtonTextAr"] = "تعرف على فريقنا",
            ["CtaButtonUrl"] = "/Home/Contact",
            ["ContactEmail"] = "about@example.test",
            ["IsPublished"] = "true",
            ["__RequestVerificationToken"] = pageToken
        }));
        Assert.Equal(HttpStatusCode.Redirect, pageSave.StatusCode);

        var about = WebUtility.HtmlDecode(await (await _client.GetAsync("/Home/About")).Content.ReadAsStringAsync());
        Assert.Contains("<title>About Updated", about);
        Assert.Contains("Updated English description", about);
        Assert.Contains("Meet our team", about);
        Assert.Contains("href=\"/Home/Contact\"", about);
        Assert.Contains("mailto:about@example.test", about);

        var aboutAr = WebUtility.HtmlDecode(await (await _client.GetAsync("/Home/About?lang=ar")).Content.ReadAsStringAsync());
        Assert.Contains("<title>عن نيوركس الجديدة", aboutAr);
        Assert.Contains("وصف عربي جديد", aboutAr);

        var settingToken = await GetTokenAsync($"/cms/settings/create?companyId={_profileId}");
        var settingSave = await _client.PostAsync("/cms/settings/create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["CompanyProfileId"] = _profileId.ToString(),
            ["Key"] = "contact.heading",
            ["Label"] = "Contact heading",
            ["SettingType"] = "text",
            ["GroupName"] = "Contact",
            ["ValueEn"] = "Send a project brief",
            ["ValueAr"] = "أرسل تفاصيل مشروعك",
            ["__RequestVerificationToken"] = settingToken
        }));
        Assert.Equal(HttpStatusCode.Redirect, settingSave.StatusCode);

        var contact = WebUtility.HtmlDecode(await (await _client.GetAsync("/Home/Contact")).Content.ReadAsStringAsync());
        Assert.Contains("Send a project brief", contact);
        Assert.Contains("أرسل تفاصيل مشروعك", contact);
    }

    [Fact]
    public async Task SharedCopyEdits_AppearInNavigationFooterAndUtilityPages()
    {
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/")).StatusCode);
        await LoginAsAdminAsync();
        var token = await GetTokenAsync($"/cms/settings/create?companyId={_profileId}");
        var saved = await _client.PostAsync("/cms/settings/create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["CompanyProfileId"] = _profileId.ToString(),
            ["Key"] = "navbar.cta",
            ["Label"] = "Navigation contact button",
            ["SettingType"] = "text",
            ["GroupName"] = "General",
            ["ValueEn"] = "Plan with Neurix",
            ["ValueAr"] = "خطط مع نيوركس",
            ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var settings = scope.ServiceProvider.GetRequiredService<ICmsSiteSettingService>();
            Assert.True((await settings.UpsertAsync(new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _profileId, Key = "footer.company.title", Label = "Footer company heading",
                GroupName = "Footer", ValueEn = "Our Company", ValueAr = "شركتنا"
            })).Success);
            Assert.True((await settings.UpsertAsync(new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _profileId, Key = "comingsoon.title", Label = "Coming soon heading",
                GroupName = "General", ValueEn = "Launching Shortly", ValueAr = "الانطلاق قريباً"
            })).Success);
            Assert.True((await settings.UpsertAsync(new CmsSiteSettingUpsertDto
            {
                CompanyProfileId = _profileId, Key = "contact.form.submit", Label = "Contact submit button",
                GroupName = "Contact", ValueEn = "Send enquiry", ValueAr = "أرسل استفسارك"
            })).Success);
        }

        var home = WebUtility.HtmlDecode(await (await _client.GetAsync("/")).Content.ReadAsStringAsync());
        Assert.Contains("Plan with Neurix", home);
        Assert.Contains("خطط مع نيوركس", home);
        Assert.Contains("Our Company", home);
        Assert.Contains("شركتنا", home);

        var soon = WebUtility.HtmlDecode(await (await _client.GetAsync("/Home/ComingSoon")).Content.ReadAsStringAsync());
        Assert.Contains("Launching Shortly", soon);
        Assert.Contains("الانطلاق قريباً", soon);

        var contact = WebUtility.HtmlDecode(await (await _client.GetAsync("/Home/Contact")).Content.ReadAsStringAsync());
        Assert.Contains("Send enquiry", contact);
        Assert.Contains("أرسل استفسارك", contact);
    }

    [Fact]
    public async Task UnpublishedHomepageSection_DisappearsInsteadOfShowingFallbackCopy()
    {
        Assert.Contains("id=\"divisions\"", await (await _client.GetAsync("/")).Content.ReadAsStringAsync());

        using (var scope = _factory.Services.CreateScope())
        {
            var sections = scope.ServiceProvider.GetRequiredService<ICmsHomeSectionService>();
            Assert.True((await sections.UpsertDivisionsSectionAsync(new CmsDivisionsSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                BadgeEn = "Hidden ecosystem",
                IsPublished = false
            })).Success);
        }

        var home = await (await _client.GetAsync("/")).Content.ReadAsStringAsync();
        Assert.DoesNotContain("id=\"divisions\"", home);
        Assert.DoesNotContain("Hidden ecosystem", home);
        Assert.Contains("id=\"hero\"", home);
    }

    [Fact]
    public async Task PublishedSectionWithNoPublishedCards_DoesNotResurrectDefaultCards()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var sections = scope.ServiceProvider.GetRequiredService<ICmsHomeSectionService>();
            Assert.True((await sections.UpsertDivisionsSectionAsync(new CmsDivisionsSectionUpsertDto
            {
                CompanyProfileId = _profileId,
                IsPublished = true
            })).Success);
        }

        var home = await (await _client.GetAsync("/")).Content.ReadAsStringAsync();
        Assert.DoesNotContain("id=\"divisions\"", home);
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
