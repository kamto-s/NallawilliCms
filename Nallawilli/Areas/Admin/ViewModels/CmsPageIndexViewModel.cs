using Nallawilli.Models.Entities;

namespace Nallawilli.Areas.Admin.ViewModels
{
    public class CmsPageIndexViewModel
    {
        public IReadOnlyList<CmsPage> Pages { get; init; } = Array.Empty<CmsPage>();

        public IReadOnlyDictionary<Guid, int> SectionCountsByPageId { get; init; } =
            new Dictionary<Guid, int>();

        public bool ShowPageStructureTools { get; init; } = true;
    }
}
