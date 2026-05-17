using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nallawilli.Models.Entities
{
    public class CmsSectionContent : BaseEntity
    {
        [Required]
        public Guid SectionId { get; set; }

        [Required]
        [StringLength(100)]
        public string ContentKey { get; set; }

        public string? ContentValue { get; set; }

        [Required]
        [StringLength(100)]
        public string InputType { get; set; }

        [Required]
        public int SortOrder { get; set; }

        [ForeignKey("SectionId")]
        public virtual CmsSection Section { get; set; }
    }
}
