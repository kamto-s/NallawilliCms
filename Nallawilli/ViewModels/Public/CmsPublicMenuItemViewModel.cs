using Nallawilli.Helpers.Public;

namespace Nallawilli.ViewModels.Public
{
    public sealed class CmsPublicMenuItemViewModel
    {
        public string Title { get; init; } = string.Empty;

        public string Slug { get; init; } = string.Empty;

        public int SortOrder { get; init; }

        public bool IsHome =>
            string.Equals(Slug, CmsPublicDefaults.HomePageSlug, StringComparison.OrdinalIgnoreCase);

        public string Url => CmsPublicDefaults.GetPageUrl(Slug);
    }
}
