using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Neurix.Common;
using Xunit;

namespace Neurix.Tests
{
    public class CmsMediaUploadHelperTests : IDisposable
    {
        private readonly string _tempWebRoot;
        private readonly FakeWebHostEnvironment _env;

        public CmsMediaUploadHelperTests()
        {
            _tempWebRoot = Path.Combine(Path.GetTempPath(), "NeurixTestUploads_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempWebRoot);

            _env = new FakeWebHostEnvironment { WebRootPath = _tempWebRoot };
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempWebRoot))
            {
                try
                {
                    Directory.Delete(_tempWebRoot, true);
                }
                catch
                {
                    // Best effort cleanup
                }
            }
        }

        [Fact]
        public async Task SaveImageAsync_WhenFileIsNull_ReturnsNull()
        {
            var result = await CmsMediaUploadHelper.SaveImageAsync(null, _env);
            Assert.Null(result);
        }

        [Fact]
        public async Task SaveImageAsync_WhenFileIsEmpty_ReturnsNull()
        {
            var fakeFile = new FakeFormFile("logo.png", Array.Empty<byte>());
            var result = await CmsMediaUploadHelper.SaveImageAsync(fakeFile, _env);
            Assert.Null(result);
        }

        [Fact]
        public async Task SaveImageAsync_WhenFileHasValidPng_SavesAndReturnsPath()
        {
            var bytes = Encoding.UTF8.GetBytes("fake-png-content");
            var fakeFile = new FakeFormFile("my logo.png", bytes);

            var result = await CmsMediaUploadHelper.SaveImageAsync(fakeFile, _env, "logos");

            Assert.NotNull(result);
            Assert.StartsWith("/uploads/cms/logos/", result);
            Assert.EndsWith(".png", result);
        }

        [Fact]
        public async Task SaveImageAsync_WhenFileHasIcoExtension_SavesAndReturnsPath()
        {
            var bytes = Encoding.UTF8.GetBytes("fake-ico-content");
            var fakeFile = new FakeFormFile("favicon.ico", bytes);

            var result = await CmsMediaUploadHelper.SaveImageAsync(fakeFile, _env, "favicons");

            Assert.NotNull(result);
            Assert.StartsWith("/uploads/cms/favicons/", result);
            Assert.EndsWith(".ico", result);
        }

        [Fact]
        public async Task SaveImageAsync_WhenFileHasSvgExtension_SavesAndReturnsPath()
        {
            var bytes = Encoding.UTF8.GetBytes("<svg></svg>");
            var fakeFile = new FakeFormFile("favicon.svg", bytes);

            var result = await CmsMediaUploadHelper.SaveImageAsync(fakeFile, _env, "favicons");

            Assert.NotNull(result);
            Assert.StartsWith("/uploads/cms/favicons/", result);
            Assert.EndsWith(".svg", result);
        }

        [Fact]
        public async Task SaveImageAsync_WhenExtensionIsDisallowed_ThrowsInvalidOperationException()
        {
            var bytes = Encoding.UTF8.GetBytes("malicious-content");
            var fakeFile = new FakeFormFile("script.exe", bytes);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CmsMediaUploadHelper.SaveImageAsync(fakeFile, _env)
            );

            Assert.Contains("Invalid file format", ex.Message);
        }

        [Fact]
        public async Task SaveImageAsync_WhenFileExceeds5Mb_ThrowsInvalidOperationException()
        {
            var fakeFile = new FakeFormFile("giant-image.jpg", 6 * 1024 * 1024);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CmsMediaUploadHelper.SaveImageAsync(fakeFile, _env)
            );

            Assert.Contains("exceeds the 5MB size limit", ex.Message);
        }

        private class FakeWebHostEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get; set; } = string.Empty;
            public IFileProvider WebRootFileProvider { get; set; } = null!;
            public string ApplicationName { get; set; } = "Neurix";
            public IFileProvider ContentRootFileProvider { get; set; } = null!;
            public string ContentRootPath { get; set; } = string.Empty;
            public string EnvironmentName { get; set; } = "Testing";
        }

        private class FakeFormFile : IFormFile
        {
            private readonly byte[] _content;
            private readonly long _length;

            public FakeFormFile(string fileName, byte[] content)
            {
                FileName = fileName;
                _content = content;
                _length = content.Length;
            }

            public FakeFormFile(string fileName, long length)
            {
                FileName = fileName;
                _content = Array.Empty<byte>();
                _length = length;
            }

            public string ContentType => "image/png";
            public string ContentDisposition => "form-data";
            public IHeaderDictionary Headers => new HeaderDictionary();
            public long Length => _length;
            public string Name => "file";
            public string FileName { get; }

            public void CopyTo(Stream target)
            {
                target.Write(_content, 0, _content.Length);
            }

            public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
            {
                await target.WriteAsync(_content, 0, _content.Length, cancellationToken);
            }

            public Stream OpenReadStream()
            {
                return new MemoryStream(_content);
            }
        }
    }
}
