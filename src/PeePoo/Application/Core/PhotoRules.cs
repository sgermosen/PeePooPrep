using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;

namespace Application.Core
{
    public static class PhotoRules
    {
        public const long MaxBytes = 10 * 1024 * 1024;
        public const string Message = "Photos must be JPG, PNG, GIF, WEBP or HEIC images up to 10 MB.";

        private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".heic", ".heif"
        };

        /// <summary>True when no file was sent, or the file looks like an acceptable image.</summary>
        public static bool IsAcceptable(IFormFile file)
        {
            if (file == null) return true;
            if (file.Length <= 0 || file.Length > MaxBytes) return false;
            return Extensions.Contains(Path.GetExtension(file.FileName ?? string.Empty))
                || (file.ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ?? false);
        }
    }
}
