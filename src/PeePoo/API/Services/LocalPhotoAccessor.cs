using Application.Interfaces;
using Application.Photos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace API.Services
{
    /// <summary>
    /// Stores photos on the local disk under wwwroot/uploads. Used when Cloudinary
    /// is not configured, so photo uploads work out of the box in development.
    /// </summary>
    public class LocalPhotoAccessor : IPhotoAccessor
    {
        public const string UploadFolder = "uploads";
        private const string IdPrefix = "local-";

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".heic", ".heif"
        };

        private static readonly Dictionary<string, string> ExtensionsByContentType = new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/gif"] = ".gif",
            ["image/webp"] = ".webp",
            ["image/heic"] = ".heic",
            ["image/heif"] = ".heif"
        };

        /// <summary>Content types for the extensions above, used when serving the files.</summary>
        public static readonly IReadOnlyDictionary<string, string> ContentTypes = new Dictionary<string, string>
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".gif"] = "image/gif",
            [".webp"] = "image/webp",
            [".heic"] = "image/heic",
            [".heif"] = "image/heif"
        };

        private readonly string _uploadPath;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalPhotoAccessor(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
            _uploadPath = Path.Combine(webRoot, UploadFolder);
        }

        public Task<PhotoUploadResult> AddPhoto(IFormFile file) => AddPhotoLargeFile(file);

        public async Task<PhotoUploadResult> AddPhotoLargeFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;
            await using var stream = file.OpenReadStream();
            return await Save(stream, file.FileName, file.ContentType);
        }

        public async Task<PhotoUploadResult> AddPhotoLarge(Stream streamdata, string FileName)
        {
            if (streamdata == null || streamdata.Length == 0) return null;
            await using var stream = streamdata;
            return await Save(stream, FileName, null);
        }

        public Task<string> DeletePhoto(string publicId)
        {
            var path = ResolvePath(publicId);
            if (path == null) return Task.FromResult<string>(null);
            if (File.Exists(path)) File.Delete(path);
            return Task.FromResult("ok");
        }

        private async Task<PhotoUploadResult> Save(Stream stream, string originalName, string contentType)
        {
            // Always store under an image extension so files are never served as active content.
            var extension = Path.GetExtension(originalName ?? string.Empty);
            if (!AllowedExtensions.Contains(extension)
                && !(contentType != null && ExtensionsByContentType.TryGetValue(contentType, out extension)))
                throw new InvalidOperationException("Only image files (jpg, png, gif, webp, heic) can be uploaded.");

            Directory.CreateDirectory(_uploadPath);
            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            await using (var output = File.Create(Path.Combine(_uploadPath, fileName)))
            {
                await stream.CopyToAsync(output);
            }

            return new PhotoUploadResult
            {
                PublicId = IdPrefix + fileName,
                Url = BuildUrl(fileName)
            };
        }

        private string ResolvePath(string publicId)
        {
            if (string.IsNullOrEmpty(publicId) || !publicId.StartsWith(IdPrefix, StringComparison.Ordinal))
                return null;
            var fileName = publicId.Substring(IdPrefix.Length);
            if (fileName != Path.GetFileName(fileName)) return null;
            return Path.Combine(_uploadPath, fileName);
        }

        private string BuildUrl(string fileName)
        {
            var relative = $"/{UploadFolder}/{fileName}";
            var request = _httpContextAccessor.HttpContext?.Request;
            return request == null ? relative : $"{request.Scheme}://{request.Host}{request.PathBase}{relative}";
        }
    }
}
