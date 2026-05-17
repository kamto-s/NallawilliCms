using Nallawilli.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nallawilli.Models.Entities
{
    public class Post : BaseEntity
    {
        [Required]
        [MaxLength(400)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Slug { get; set; } = string.Empty;


        [Required]
        public string Content { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Author { get; set; }

        [MaxLength(1000)]
        public string? Thumbnail { get; set; }

        public ContentStatus Status { get; set; } = ContentStatus.Draft;

        public DateTime? PublishedAt { get; set; }

        public DateTime? ScheduledPublishAt { get; set; }

        public Guid CategoryId { get; set; }

        public Category? Category { get; set; }
    }
}
