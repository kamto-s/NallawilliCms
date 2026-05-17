using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Nallawilli.Helpers.Admin
{
    public static class CmsAdminImageStorage
    {
        public const long MaxBytes = 5 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions =
        [
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        ];

        public const string WebPrefixPages = "/uploads/cms/pages/";
        public const string WebPrefixSections = "/uploads/cms/sections/";

        public static async Task<(string? webPath, string? error)> TrySaveAsync(
            IWebHostEnvironment env,
            IFormFile? file,
            string webPathPrefix,
            CancellationToken cancellationToken = default)
        {
            if (file is null || file.Length == 0)
                return (null, null);

            if (file.Length > MaxBytes)
                return (null, "Image must be 5 MB or smaller.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                return (null, "Allowed image types: JPG, PNG, GIF, WebP.");

            var relativeDir = webPathPrefix.TrimStart('/').TrimEnd('/');
            var dir = Path.Combine(env.WebRootPath, relativeDir.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(dir);

            var name = $"{Guid.NewGuid():N}{ext}";
            var physical = Path.Combine(dir, name);

            await using (var stream = File.Create(physical))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            return (webPathPrefix + name, null);
        }

        public static void TryDeleteIfManaged(IWebHostEnvironment env, string? webPath, string webPathPrefix)
        {
            if (string.IsNullOrEmpty(webPath) ||
                !webPath.StartsWith(webPathPrefix, StringComparison.OrdinalIgnoreCase))
                return;

            var relative = webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physical = Path.Combine(env.WebRootPath, relative);
            if (File.Exists(physical))
                File.Delete(physical);
        }
    }
}
