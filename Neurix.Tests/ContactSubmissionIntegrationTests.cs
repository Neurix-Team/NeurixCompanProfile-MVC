using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Neurix.BLL.Common;
using Neurix.BLL.Dtos;
using Neurix.BLL.Services;
using Neurix.DAL.Data;
using Neurix.DAL.Enums;
using Neurix.Tests.Infrastructure;
using Xunit;

namespace Neurix.Tests
{
    /// <summary>
    /// HTTP-level tests for NQ-013: these go through real routing, model binding and
    /// anti-forgery validation, which HomeControllerTests (direct controller calls)
    /// cannot exercise.
    /// </summary>
    public class ContactSubmissionIntegrationTests : IDisposable
    {
        private readonly NeurixWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public ContactSubmissionIntegrationTests()
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

        private async Task<string> GetAntiForgeryTokenAsync()
        {
            var response = await _client.GetAsync("/Home/Contact");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
            Assert.True(match.Success, "Anti-forgery token not found on the rendered Contact page.");
            return match.Groups[1].Value;
        }

        [Fact]
        public async Task Post_ValidSubmissionWithoutCompanyName_SavesLeadAndRedirectsToSuccess()
        {
            var token = await GetAntiForgeryTokenAsync();
            var form = new Dictionary<string, string>
            {
                ["FullName"] = "Sara Ahmed",
                ["Email"] = "sara.ahmed@example.com",
                ["InquiryType"] = "Demo",
                ["Message"] = "Interested in a product demo.",
                ["__RequestVerificationToken"] = token
            };

            var response = await _client.PostAsync("/Home/Contact", new FormUrlEncodedContent(form));

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Home/Contact", response.Headers.Location?.OriginalString);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
            var lead = db.Leads.Single(l => l.Email == "sara.ahmed@example.com");
            Assert.Equal("Sara Ahmed", lead.FullName);
            Assert.Null(lead.CompanyName);
            Assert.Equal(LeadSource.Website, lead.Source);
        }

        [Fact]
        public async Task Post_WithWhitespaceCompanyName_StoresNullNotWhitespace()
        {
            var token = await GetAntiForgeryTokenAsync();
            var form = new Dictionary<string, string>
            {
                ["FullName"] = "Omar Khaled",
                ["Email"] = "omar.khaled@example.com",
                ["CompanyName"] = "   ",
                ["InquiryType"] = "Quote",
                ["Message"] = "Need pricing.",
                ["__RequestVerificationToken"] = token
            };

            var response = await _client.PostAsync("/Home/Contact", new FormUrlEncodedContent(form));

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
            var lead = db.Leads.Single(l => l.Email == "omar.khaled@example.com");
            Assert.Null(lead.CompanyName);
        }

        [Fact]
        public async Task Post_WithoutAntiForgeryToken_ReturnsBadRequestAndDoesNotSaveLead()
        {
            var form = new Dictionary<string, string>
            {
                ["FullName"] = "No Token Visitor",
                ["Email"] = "notoken@example.com",
                ["InquiryType"] = "Demo",
                ["Message"] = "Should be rejected before reaching the database."
            };

            var response = await _client.PostAsync("/Home/Contact", new FormUrlEncodedContent(form));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
            Assert.False(db.Leads.Any(l => l.Email == "notoken@example.com"));
        }

        [Fact]
        public async Task Post_InvalidEmail_ReturnsFormWithEnteredValuesAndDoesNotSaveLead()
        {
            var token = await GetAntiForgeryTokenAsync();
            var form = new Dictionary<string, string>
            {
                ["FullName"] = "Bad Email Visitor",
                ["Email"] = "not-an-email",
                ["CompanyName"] = "Acme",
                ["InquiryType"] = "Support",
                ["Message"] = "This should fail validation.",
                ["__RequestVerificationToken"] = token
            };

            var response = await _client.PostAsync("/Home/Contact", new FormUrlEncodedContent(form));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = await response.Content.ReadAsStringAsync();
            Assert.Contains("Bad Email Visitor", html);
            Assert.Contains("Acme", html);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
            Assert.False(db.Leads.Any(l => l.FullName == "Bad Email Visitor"));
        }

        [Fact]
        public async Task Post_InvalidInquiryType_IsRejectedByServerSideValidation()
        {
            var token = await GetAntiForgeryTokenAsync();
            var form = new Dictionary<string, string>
            {
                ["FullName"] = "Tampered Select Visitor",
                ["Email"] = "tampered@example.com",
                ["InquiryType"] = "NotARealOption",
                ["Message"] = "Simulates a hand-edited request bypassing the <select>.",
                ["__RequestVerificationToken"] = token
            };

            var response = await _client.PostAsync("/Home/Contact", new FormUrlEncodedContent(form));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
            Assert.False(db.Leads.Any(l => l.Email == "tampered@example.com"));
        }

        [Fact]
        public async Task Post_WhenLeadCaptureFails_DoesNotShowSuccessAndDoesNotSaveLead()
        {
            using var throwingFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<ILeadService>();
                    services.AddScoped<ILeadService, ThrowingLeadService>();
                });
            });

            using (var scope = throwingFactory.Services.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<CrmDbContext>().Database.EnsureCreated();
                scope.ServiceProvider.GetRequiredService<CmsDbContext>().Database.EnsureCreated();
            }

            using var throwingClient = throwingFactory.CreateClient(
                new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            var tokenResponse = await throwingClient.GetAsync("/Home/Contact");
            tokenResponse.EnsureSuccessStatusCode();
            var tokenHtml = await tokenResponse.Content.ReadAsStringAsync();
            var token = Regex.Match(tokenHtml, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;

            var form = new Dictionary<string, string>
            {
                ["FullName"] = "Failed Capture Visitor",
                ["Email"] = "failedcapture@example.com",
                ["InquiryType"] = "Demo",
                ["Message"] = "This save should fail and must not show success.",
                ["__RequestVerificationToken"] = token
            };

            var response = await throwingClient.PostAsync("/Home/Contact", new FormUrlEncodedContent(form));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = await response.Content.ReadAsStringAsync();
            Assert.Contains("went wrong", html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Thank you for reaching out", html);
            Assert.Contains("Failed Capture Visitor", html);

            using var assertScope = throwingFactory.Services.CreateScope();
            var db = assertScope.ServiceProvider.GetRequiredService<CrmDbContext>();
            Assert.False(db.Leads.Any(l => l.Email == "failedcapture@example.com"));
        }

        private class ThrowingLeadService : ILeadService
        {
            public Task<Guid> CaptureWebsiteEnquiryAsync(WebsiteEnquiry enquiry) =>
                throw new InvalidOperationException("Simulated CRM outage for NQ-013 verification.");

            public Task<PagedResult<LeadListRow>> GetListAsync(string? search, LeadStatus? status, Guid? assignedToUserId, int page, int pageSize = PagingDefaults.DefaultPageSize, bool unassignedOnly = false)
                => throw new NotImplementedException();
            public Task<LeadDetail?> GetDetailAsync(Guid id) => throw new NotImplementedException();
            public Task<LeadRequest?> GetForEditAsync(Guid id) => throw new NotImplementedException();
            public Task<Guid> CreateAsync(LeadRequest request, Guid currentUserId) => throw new NotImplementedException();
            public Task<LeadUpdateOutcome> UpdateAsync(Guid id, LeadRequest request) => throw new NotImplementedException();
            public Task<ServiceResult> SoftDeleteAsync(Guid id) => throw new NotImplementedException();
            public Task<IReadOnlyList<AssignableUser>> GetAssignableUsersAsync() => throw new NotImplementedException();
        }
    }
}
