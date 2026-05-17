using System.ComponentModel.DataAnnotations;

namespace Nallawilli.Areas.Admin.ViewModels
{
    public class CmsSectionFormViewModel
    {
        public Guid Id { get; set; }

        public Guid PageId { get; set; }

        public string PageTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Section code is required.")]
        [StringLength(100)]
        [RegularExpression(
            @"^[a-z_][a-z0-9_-]*$",
            ErrorMessage = "Use lowercase letters, numbers, underscores, or hyphens; must start with a letter or underscore.")]
        [Display(Name = "Section code")]
        public string SectionCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Section name is required.")]
        [StringLength(300)]
        [Display(Name = "Section name")]
        public string SectionName { get; set; } = string.Empty;

        [Display(Name = "Sort order")]
        public int SortOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
