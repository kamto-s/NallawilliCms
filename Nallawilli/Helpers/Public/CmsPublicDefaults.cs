namespace Nallawilli.Helpers.Public
{
    public static class CmsPublicDefaults
    {
        public const string HomePageSlug = "home";

        /// <summary>URL segment for CMS pages, e.g. /p/about.</summary>
        public const string PagePathPrefix = "p";

        public static string GetPageUrl(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)
                || string.Equals(slug, HomePageSlug, StringComparison.OrdinalIgnoreCase))
                return "/";

            return $"/{PagePathPrefix}/{slug.Trim().ToLowerInvariant()}";
        }
    }
}
