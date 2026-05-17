using System.ComponentModel.DataAnnotations;

namespace Nallawilli.Areas.Admin.ViewModels
{
    public class CmsSectionContentFormViewModel
    {
        public Guid Id { get; set; }

        public Guid SectionId { get; set; }

        public Guid PageId { get; set; }

        public string PageTitle { get; set; } = string.Empty;

        public string SectionName { get; set; } = string.Empty;

        public string SectionCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content key is required.")]
        [StringLength(100)]
        [RegularExpression(
            @"^[a-z_][a-z0-9_-]*$",
            ErrorMessage = "Use lowercase letters, numbers, underscores, or hyphens; must start with a letter or underscore.")]
        [Display(Name = "Content key")]
        public string ContentKey { get; set; } = string.Empty;

        [Display(Name = "Content value")]
        public string? ContentValue { get; set; }

        [Required(ErrorMessage = "Input type is required.")]
        [StringLength(100)]
        [Display(Name = "Input type")]
        public string InputType { get; set; } = "text";

        [Display(Name = "Sort order")]
        public int SortOrder { get; set; }
    }
}
