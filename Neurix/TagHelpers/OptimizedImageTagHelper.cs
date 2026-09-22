using System.Net;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Neurix.TagHelpers;

[HtmlTargetElement("img", Attributes = "src")]
public sealed class OptimizedImageTagHelper(IWebHostEnvironment environment, IFileVersionProvider versions) : TagHelper
{
    public override int Order => 110;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var value = output.Attributes["src"]?.Value;
        string? source;
        if (value is IHtmlContent html)
        {
            using var writer = new StringWriter();
            html.WriteTo(writer, HtmlEncoder.Default);
            source = WebUtility.HtmlDecode(writer.ToString());
        }
        else
        {
            source = value?.ToString();
        }
        if (source?.StartsWith("~/images/", StringComparison.Ordinal) == true) source = source[1..];
        if (string.IsNullOrEmpty(source) || !source.StartsWith("/images/", StringComparison.Ordinal)) return;
        if (source.Contains('?') || source.Contains('#') || source.Contains("..")) return;
        source = Uri.UnescapeDataString(source);
        if (source.Contains("..") || source.Contains('\\')) return;
        var extension = Path.GetExtension(source);
        if (!extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
            && !extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            && !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) return;

        var optimized = Path.ChangeExtension(source, ".webp");
        if (!environment.WebRootFileProvider.GetFileInfo(optimized.TrimStart('/')).Exists) return;

        output.Attributes.SetAttribute("src", versions.AddFileVersionToPath("", optimized));
        if (!output.Attributes.ContainsName("decoding")) output.Attributes.SetAttribute("decoding", "async");
    }
}
