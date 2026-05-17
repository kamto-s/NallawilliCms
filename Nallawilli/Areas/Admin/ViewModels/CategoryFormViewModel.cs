using System.ComponentModel.DataAnnotations;

namespace Nallawilli.Areas.Admin.ViewModels
{
    public class CategoryFormViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(200)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
