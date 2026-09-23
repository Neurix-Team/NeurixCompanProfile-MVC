using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Neurix.BLL.Common;
using Neurix.DAL.Models;
using Neurix.Tests.Infrastructure;

namespace Neurix.Tests;

public sealed class DashboardAccessIntegrationTests : IDisposable
{
    private const string Password = "Str0ng!Passw0rd";
    private readonly NeurixWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public DashboardAccessIntegrationTests()
    {
        _factory.EnsureDatabasesCreated();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task AnonymousVisitorCannotOpenEitherDashboard()
    {
        foreach (var path in new[] { "/cms", "/crm", "/cms/companies", "/crm/leads" })
        {
            var response = await _client.GetAsync(path);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/crm/account/login", response.Headers.Location!.OriginalString);
        }
    }

    [Theory]
    [InlineData(CrmRoles.CrmStaff, false)]
    [InlineData(CrmRoles.Admin, true)]
    public async Task RoleControlsCmsWhileBothRolesCanOpenCrm(string role, bool cmsAllowed)
    {
        var email = $"{role.ToLowerInvariant()}@example.test";
        using (var scope = _factory.Services.CreateScope())
        {
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.True((await roles.CreateAsync(new ApplicationRole { Name = role })).Succeeded);
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = role };
            Assert.True((await users.CreateAsync(user, Password)).Succeeded);
            Assert.True((await users.AddToRoleAsync(user, role)).Succeeded);
        }

        var loginPage = await _client.GetAsync("/crm/account/login");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(token);
        var login = await _client.PostAsync("/crm/account/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = Password,
            ["__RequestVerificationToken"] = token
        }));
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/crm")).StatusCode);
        var cms = await _client.GetAsync("/cms");
        if (cmsAllowed)
        {
            Assert.Equal(HttpStatusCode.OK, cms.StatusCode);
        }
        else
        {
            Assert.Equal(HttpStatusCode.Redirect, cms.StatusCode);
            Assert.Contains("/crm/account/access-denied", cms.Headers.Location!.OriginalString);
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
