using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Neurix.Common
{
    public static class CmsMediaUploadHelper
    {
        private static readonly string[] AllowedExtensions = { ".png", ".jpg", ".jpeg", ".webp", ".svg", ".ico" };
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

        /// <summary>
        /// Saves an uploaded image to wwwroot/uploads/cms/ and returns the relative web path (e.g. /uploads/cms/filename.png).
        /// Returns null if file is null or empty.
        /// Throws InvalidOperationException if validation fails.
        /// </summary>
        public static async Task<string?> SaveImageAsync(IFormFile? file, IWebHostEnvironment environment, string subfolder = "")
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            if (file.Length > MaxFileSizeInBytes)
            {
                throw new InvalidOperationException("Uploaded image exceeds the 5MB size limit.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"Invalid file format '{extension}'. Allowed formats: {string.Join(", ", AllowedExtensions)}.");
            }

            var uploadsRoot = Path.Combine(environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "cms", subfolder);
            if (!Directory.Exists(uploadsRoot))
            {
                Directory.CreateDirectory(uploadsRoot);
            }

            var cleanFileName = Path.GetFileNameWithoutExtension(file.FileName)
                .Replace(" ", "-")
                .Replace("..", "")
                .Trim();

            // Truncate to reasonable length
            if (cleanFileName.Length > 40)
            {
                cleanFileName = cleanFileName[..40];
            }

            var uniqueFileName = $"{cleanFileName}_{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadsRoot, uniqueFileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = string.IsNullOrWhiteSpace(subfolder)
                ? $"/uploads/cms/{uniqueFileName}"
                : $"/uploads/cms/{subfolder.Trim('/')}/{uniqueFileName}";

            return relativePath.Replace("\\", "/");
        }
    }
}
