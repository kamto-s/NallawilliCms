using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nallawilli.Models.Entities
{
    public class CmsSection : BaseEntity
    {
        [Required]
        public Guid PageId { get; set; }

        [Required]
        [StringLength(100)]
        public string SectionCode { get; set; }

        [Required]
        [StringLength(300)]
        public string SectionName { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public int SortOrder { get; set; }

        [ForeignKey("PageId")]
        public virtual CmsPage Page { get; set; }

        public virtual ICollection<CmsSectionContent> SectionContents { get; set; } = new List<CmsSectionContent>();
    }
}