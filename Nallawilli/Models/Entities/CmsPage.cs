using System.ComponentModel.DataAnnotations;

namespace Nallawilli.Models.Entities
{
    public class CmsPage : BaseEntity
    {
        [Required]
        [StringLength(300)]
        public string Title { get; set; }

        [Required]
        [StringLength(300)]
        public string Slug { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool ShowInMenu { get; set; } = true;
        
        [Required]
        public int SortOrder { get; set; } = 0;

        [StringLength(300)]
        public string? MetaTitle { get; set; }

        [StringLength(500)]
        public string? MetaDescription { get; set; }
        
        public virtual ICollection<CmsSection> Sections { get; set; } = new List<CmsSection>();
    }
}
