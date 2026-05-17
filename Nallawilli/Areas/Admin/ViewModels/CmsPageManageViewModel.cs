namespace Nallawilli.Areas.Admin.ViewModels
{
    public class CmsPageManageViewModel
    {
        public Guid PageId { get; set; }

        public string PageTitle { get; set; } = string.Empty;

        public string PageSlug { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public List<CmsSectionManageViewModel> Sections { get; set; } = new();
    }

    public class CmsSectionManageViewModel
    {
        public Guid SectionId { get; set; }

        public string SectionCode { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public List<CmsContentFieldViewModel> Fields { get; set; } = new();
    }

    public class CmsContentFieldViewModel
    {
        public Guid ContentId { get; set; }

        public string ContentKey { get; set; } = string.Empty;

        public string? ContentValue { get; set; }

        public string InputType { get; set; } = "text";

        public int SortOrder { get; set; }
    }
}
