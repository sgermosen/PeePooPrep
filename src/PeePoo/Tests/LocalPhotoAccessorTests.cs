using API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class LocalPhotoAccessorTests : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "peepoo-tests-" + Guid.NewGuid().ToString("N"));
        private readonly LocalPhotoAccessor _accessor;

        public LocalPhotoAccessorTests()
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("localhost:5093");
            _accessor = new LocalPhotoAccessor(new FakeWebHostEnvironment { WebRootPath = _root },
                new HttpContextAccessor { HttpContext = context });
        }

        private static IFormFile Image(string name, string contentType = null)
        {
            var bytes = new byte[] { 1, 2, 3, 4 };
            return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "File", name)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        [Fact]
        public async Task Upload_SavesFile_AndReturnsAbsoluteUrl()
        {
            var result = await _accessor.AddPhotoLargeFile(Image("photo.JPG"));

            Assert.StartsWith("local-", result.PublicId);
            Assert.StartsWith("http://localhost:5093/uploads/", result.Url);
            Assert.EndsWith(".jpg", result.Url);
            var fileName = result.Url.Substring(result.Url.LastIndexOf('/') + 1);
            Assert.True(File.Exists(Path.Combine(_root, "uploads", fileName)));
        }

        [Fact]
        public async Task Upload_RejectsNonImageFiles()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _accessor.AddPhotoLargeFile(Image("evil.html")));
        }

        [Fact]
        public async Task Upload_UsesContentType_WhenFileNameHasNoImageExtension()
        {
            var result = await _accessor.AddPhotoLargeFile(Image("IMG_0001", "image/jpeg"));

            Assert.EndsWith(".jpg", result.Url);
        }

        [Fact]
        public async Task Upload_RejectsHtml_WhenContentTypeIsNotAnImage()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _accessor.AddPhotoLargeFile(Image("evil.html", "text/html")));
        }

        [Fact]
        public async Task Delete_RemovesFile_AndIgnoresPathTraversal()
        {
            var result = await _accessor.AddPhoto(Image("photo.png"));
            var fileName = result.Url.Substring(result.Url.LastIndexOf('/') + 1);

            Assert.Null(await _accessor.DeletePhoto("local-../secret.png"));
            Assert.Equal("ok", await _accessor.DeletePhoto(result.PublicId));
            Assert.False(File.Exists(Path.Combine(_root, "uploads", fileName)));
        }

        public void Dispose()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        private class FakeWebHostEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get; set; }
            public IFileProvider WebRootFileProvider { get; set; }
            public string ApplicationName { get; set; } = "Tests";
            public IFileProvider ContentRootFileProvider { get; set; }
            public string ContentRootPath { get; set; } = Path.GetTempPath();
            public string EnvironmentName { get; set; } = "Development";
        }
    }
}
