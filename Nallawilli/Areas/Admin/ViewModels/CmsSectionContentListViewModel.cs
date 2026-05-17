using Nallawilli.Models.Entities;

namespace Nallawilli.Areas.Admin.ViewModels
{
    public class CmsSectionContentListViewModel
    {
        public CmsPage Page { get; set; } = null!;

        public CmsSection Section { get; set; } = null!;

        public IReadOnlyList<CmsSectionContent> Contents { get; init; } =
            Array.Empty<CmsSectionContent>();
    }
}
