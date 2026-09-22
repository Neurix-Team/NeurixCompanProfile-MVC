using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Neurix.DAL.Models;
using Neurix.Tests.Infrastructure;
using Xunit;

namespace Neurix.Tests
{
    /// <summary>
    /// HTTP-level tests for NQ-026: CrmProfileController is a separate, [Authorize]-only
    /// controller from the [AllowAnonymous] CrmAccountController, so anonymous access and
    /// the full sign-in -> change-password -> re-authenticate cycle need the real pipeline,
    /// not a direct controller call.
    /// </summary>
    public class CrmProfileIntegrationTests : IDisposable
    {
        private const string Password = "Str0ng!Passw0rd";
        private readonly NeurixWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public CrmProfileIntegrationTests()
        {
            _factory = new NeurixWebApplicationFactory();
            _factory.EnsureDatabasesCreated();
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private async Task<ApplicationUser> SeedUserAsync(string email, string fullName)
        {
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = fullName };
            var result = await userManager.CreateAsync(user, Password);
            Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Description)));
            return user;
        }

        private async Task<string> GetTokenAsync(string path)
        {
            var response = await _client.GetAsync(path);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
            Assert.True(match.Success, $"Anti-forgery token not found on {path}.");
            return match.Groups[1].Value;
        }

        private async Task LoginAsync(string email, string password = Password)
        {
            var token = await GetTokenAsync("/crm/account/login");
            var form = new Dictionary<string, string>
            {
                ["Email"] = email,
                ["Password"] = password,
                ["__RequestVerificationToken"] = token
            };
            var response = await _client.PostAsync("/crm/account/login", new FormUrlEncodedContent(form));
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Fact]
        public async Task Get_Profile_AnonymousVisitor_IsRedirectedToLogin_NotServed()
        {
            var response = await _client.GetAsync("/crm/profile");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/crm/account/login", response.Headers.Location!.OriginalString);
        }

        [Fact]
        public async Task Post_UpdateProfile_ChangesFullName()
        {
            await SeedUserAsync("staff@example.com", "Original Name");
            await LoginAsync("staff@example.com");

            var token = await GetTokenAsync("/crm/profile");
            var form = new Dictionary<string, string>
            {
                ["FullName"] = "Updated Name",
                ["__RequestVerificationToken"] = token
            };
            var response = await _client.PostAsync("/crm/profile/update", new FormUrlEncodedContent(form));

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync("staff@example.com");
            Assert.Equal("Updated Name", user!.FullName);
        }

        [Fact]
        public async Task Post_ChangePassword_WithWrongCurrentPassword_DoesNotChangeIt()
        {
            await SeedUserAsync("staff2@example.com", "Staff Two");
            await LoginAsync("staff2@example.com");

            var token = await GetTokenAsync("/crm/profile/change-password");
            var form = new Dictionary<string, string>
            {
                ["CurrentPassword"] = "WrongPassword1!",
                ["NewPassword"] = "Another$trongPass1",
                ["ConfirmPassword"] = "Another$trongPass1",
                ["__RequestVerificationToken"] = token
            };
            var response = await _client.PostAsync("/crm/profile/change-password", new FormUrlEncodedContent(form));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = await response.Content.ReadAsStringAsync();
            Assert.Contains("not correct", html);

            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync("staff2@example.com");
            Assert.True(await userManager.CheckPasswordAsync(user!, Password));
        }

        [Fact]
        public async Task Post_ChangePassword_WithValidCurrentPassword_ChangesItAndOldPasswordStopsWorking()
        {
            await SeedUserAsync("staff3@example.com", "Staff Three");
            await LoginAsync("staff3@example.com");

            const string newPassword = "Br4nd$NewPassword";
            var token = await GetTokenAsync("/crm/profile/change-password");
            var form = new Dictionary<string, string>
            {
                ["CurrentPassword"] = Password,
                ["NewPassword"] = newPassword,
                ["ConfirmPassword"] = newPassword,
                ["__RequestVerificationToken"] = token
            };
            var response = await _client.PostAsync("/crm/profile/change-password", new FormUrlEncodedContent(form));

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync("staff3@example.com");
            Assert.False(await userManager.CheckPasswordAsync(user!, Password));
            Assert.True(await userManager.CheckPasswordAsync(user!, newPassword));
        }

        [Fact]
        public async Task Post_UpdateProfile_CannotBeUsedToEditAnotherUsersId()
        {
            // The form has no user-id field at all; the controller reads it from the
            // authenticated claim. This proves the request cannot influence whose row
            // is written even if a caller adds an Id-shaped field to the POST body.
            var owner = await SeedUserAsync("owner@example.com", "Owner");
            var other = await SeedUserAsync("other@example.com", "Other Person");
            await LoginAsync("owner@example.com");

            var token = await GetTokenAsync("/crm/profile");
            var form = new Dictionary<string, string>
            {
                ["Id"] = other.Id.ToString(),
                ["FullName"] = "Hijacked Name",
                ["__RequestVerificationToken"] = token
            };
            await _client.PostAsync("/crm/profile/update", new FormUrlEncodedContent(form));

            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var otherReloaded = await userManager.FindByIdAsync(other.Id.ToString());
            Assert.Equal("Other Person", otherReloaded!.FullName);
        }
    }
}
