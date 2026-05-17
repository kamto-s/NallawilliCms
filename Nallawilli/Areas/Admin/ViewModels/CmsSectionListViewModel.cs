using Nallawilli.Models.Entities;

namespace Nallawilli.Areas.Admin.ViewModels
{
    public class CmsSectionListViewModel
    {
        public CmsPage Page { get; set; } = null!;

        public IReadOnlyList<CmsSection> Sections { get; set; } = Array.Empty<CmsSection>();

        public IReadOnlyDictionary<Guid, int> ContentCountsBySectionId { get; init; } =
            new Dictionary<Guid, int>();
    }
}
