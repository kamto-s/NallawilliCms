namespace Nallawilli.ViewModels
{
    public sealed class CmsPublicPageViewModel
    {
        public string Slug { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;

        public string? PageTitle { get; init; }

        public string? MetaDescription { get; init; }

        public IReadOnlyList<CmsPublicSectionViewModel> Sections { get; init; } =
            Array.Empty<CmsPublicSectionViewModel>();
    }
}
