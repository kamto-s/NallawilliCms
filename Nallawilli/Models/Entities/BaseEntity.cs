using System.ComponentModel.DataAnnotations;

namespace Nallawilli.Models.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime? CreatedAt { get; set; }

        [MaxLength(256)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(256)]
        public string? UpdatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }

        [MaxLength(256)]
        public string? DeletedBy { get; set; }
    }
}
