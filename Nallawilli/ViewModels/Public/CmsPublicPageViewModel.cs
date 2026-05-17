namespace Nallawilli.ViewModels.Public
{
    public sealed class CmsPublicPageViewModel
    {
        public string Slug { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string? MetaTitle { get; init; }

        public string? MetaDescription { get; init; }

        public IReadOnlyList<CmsPublicSectionViewModel> Sections { get; init; } =
            Array.Empty<CmsPublicSectionViewModel>();
    }
}
