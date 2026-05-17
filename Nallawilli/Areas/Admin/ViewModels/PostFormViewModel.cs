using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nallawilli.Models.Enums;

namespace Nallawilli.Areas.Admin.ViewModels
{
    public class PostFormViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(400)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required.")]
        [Display(Name = "Content")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Category")]
        public Guid CategoryId { get; set; }

        [Display(Name = "Status")]
        public ContentStatus Status { get; set; } = ContentStatus.Draft;

        [Display(Name = "Scheduled publish")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? ScheduledPublishAt { get; set; }

        [MaxLength(1000)]
        public string? Thumbnail { get; set; }

        [Display(Name = "Thumbnail image")]
        public IFormFile? ThumbnailFile { get; set; }

        public List<SelectListItem> CategoryOptions { get; set; } = new();
    }
}
