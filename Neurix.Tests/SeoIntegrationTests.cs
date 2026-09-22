using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Neurix.Tests.Infrastructure;
using Xunit;

namespace Neurix.Tests
{
    /// <summary>
    /// HTTP-level tests for NQ-009 and NQ-010: a body-less 404 becomes a real page,
    /// and robots.txt/sitemap.xml are served with the back-office paths excluded.
    /// </summary>
    public class SeoIntegrationTests : IDisposable
    {
        private readonly NeurixWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public SeoIntegrationTests()
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

        [Fact]
        public async Task RobotsTxt_ExcludesDashboards_AndPointsToSitemap()
        {
            var response = await _client.GetAsync("/robots.txt");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("User-agent: *", body);
            Assert.Contains("Disallow: /crm", body);
            Assert.Contains("Disallow: /cms", body);
            Assert.Contains("Sitemap: http", body);
            Assert.EndsWith("/sitemap.xml", body.TrimEnd());
        }

        [Fact]
        public async Task SitemapXml_ListsPublicPages_WithoutDashboardRoutes()
        {
            var response = await _client.GetAsync("/sitemap.xml");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/xml", response.Content.Headers.ContentType?.MediaType);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("<urlset", body);
            Assert.Contains("/Home/Contact", body);
            Assert.Contains("/Home/Privacy", body);
            Assert.DoesNotContain("/crm", body);
            Assert.DoesNotContain("/cms", body);
        }

        [Fact]
        public async Task UnknownPage_Returns404WithFriendlyBody()
        {
            var response = await _client.GetAsync("/Home/This-Page-Does-Not-Exist");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Page Not Found", body);
        }

        [Fact]
        public async Task ShowStatusCode_With404_ReturnsNotFoundView()
        {
            var response = await _client.GetAsync("/Home/ShowStatusCode?code=404");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Page Not Found", body);
        }
    }
}
