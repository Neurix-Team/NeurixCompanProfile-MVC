using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Neurix.Controllers
{
    /// <summary>
    /// robots.txt and sitemap.xml served from code so the Sitemap directive always
    /// matches the host the site is actually served on. CMS-managed detail pages
    /// (portfolio projects, insights posts) are intentionally not enumerated yet:
    /// their slugs are content, not code, so a hardcoded list would go stale silently.
    /// </summary>
    public class SeoController : Controller
    {
        private static readonly string[] PublicPaths =
        {
            "/",
            "/Home/Labs",
            "/Home/Technology",
            "/Home/HQ",
            "/Home/Plus",
            "/Home/Club",
            "/Home/AI",
            "/Home/About",
            "/Home/Portfolio",
            "/Home/Insights",
            "/Home/Contact",
            "/Home/Privacy",
            "/Home/Terms",
            "/Home/Security"
        };

        [HttpGet("/robots.txt")]
        public IActionResult Robots()
        {
            var lines = new[]
            {
                "User-agent: *",
                "Allow: /",
                "Disallow: /crm",
                "Disallow: /cms",
                "Disallow: /api",
                "Disallow: /ws",
                string.Empty,
                $"Sitemap: {BaseUrl()}/sitemap.xml"
            };

            return Content(string.Join("\n", lines), "text/plain; charset=utf-8");
        }

        [HttpGet("/sitemap.xml")]
        public IActionResult Sitemap()
        {
            var lastModified = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            foreach (var path in PublicPaths)
            {
                xml.AppendLine("  <url>");
                xml.AppendLine($"    <loc>{BaseUrl()}{path}</loc>");
                xml.AppendLine($"    <lastmod>{lastModified}</lastmod>");
                xml.AppendLine("  </url>");
            }

            xml.AppendLine("</urlset>");

            return Content(xml.ToString(), "application/xml; charset=utf-8");
        }

        private string BaseUrl() => $"{Request.Scheme}://{Request.Host}";
    }
}
