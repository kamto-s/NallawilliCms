namespace Nallawilli.Helpers.Public
{
    public static class CmsMediaPaths
    {
        /// <summary>Turns stored CMS path (/uploads/...) into app-relative path for Url.Content.</summary>
        public static string? ToAppRelative(string? storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
                return null;

            var path = storedPath.Trim();
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return path;

            if (path.StartsWith("~/", StringComparison.Ordinal))
                return path;

            if (path.StartsWith('/'))
                return "~" + path;

            return "~/" + path;
        }
    }
}
