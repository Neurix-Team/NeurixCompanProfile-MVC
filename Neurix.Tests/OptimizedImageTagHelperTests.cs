using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.FileProviders;
using Neurix.TagHelpers;

namespace Neurix.Tests;

public sealed class OptimizedImageTagHelperTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    [Theory]
    [InlineData("/images/Mission & Vision.png", false)]
    [InlineData("~/images/Mission & Vision.png", false)]
    [InlineData("/images/Mission &amp; Vision.png", true)]
    [InlineData("/images/Mission%20%26%20Vision.png", false)]
    public void UsesExistingWebpForRazorAndResolvedUrls(string source, bool encoded)
    {
        Directory.CreateDirectory(Path.Combine(root, "images"));
        File.WriteAllText(Path.Combine(root, "images", "Mission & Vision.webp"), "fixture");
        using var files = new PhysicalFileProvider(root);
        var output = Process(encoded ? new HtmlString(source) : source, files);

        Assert.Equal("/images/Mission & Vision.webp?v=fixture", output.Attributes["src"].Value);
        Assert.Equal("async", output.Attributes["decoding"].Value);
    }

    [Theory]
    [InlineData("https://example.com/images/Mission.png")]
    [InlineData("/uploads/Mission.png")]
    [InlineData("/images/missing.png")]
    [InlineData("/images/Mission.png?width=200")]
    [InlineData("/images/%2e%2e/private.png")]
    public void LeavesUnrelatedOrMissingImagesUnchanged(string source)
    {
        var output = Process(source, new NullFileProvider());
        Assert.Equal(source, output.Attributes["src"].Value);
    }

    private static TagHelperOutput Process(object source, IFileProvider files)
    {
        var helper = new OptimizedImageTagHelper(new TestEnvironment { WebRootFileProvider = files }, new Versions());
        var attrs = new TagHelperAttributeList { new("src", source) };
        var context = new TagHelperContext(attrs, new Dictionary<object, object>(), "image");
        var output = new TagHelperOutput("img", attrs, (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        helper.Process(context, output);
        return output;
    }

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }

    private sealed class Versions : IFileVersionProvider
    {
        public string AddFileVersionToPath(PathString requestPathBase, string path) => path + "?v=fixture";
    }

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Neurix.Tests";
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
