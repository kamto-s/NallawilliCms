using System.ComponentModel.DataAnnotations;

namespace Nallawilli.Areas.Admin.ViewModels
{
    public class CmsPageFormViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(300)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Show in menu")]
        public bool ShowInMenu { get; set; } = true;

        [Display(Name = "Sort order")]
        public int SortOrder { get; set; }

        [MaxLength(300)]
        [Display(Name = "Meta title")]
        public string? MetaTitle { get; set; }

        [MaxLength(500)]
        [Display(Name = "Meta description")]
        public string? MetaDescription { get; set; }

        /// <summary>Read-only display on edit; URL segment is still derived from title on save.</summary>
        [Display(Name = "Current URL slug")]
        public string? Slug { get; set; }
    }
}
