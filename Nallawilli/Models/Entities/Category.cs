using System.ComponentModel.DataAnnotations;

namespace Nallawilli.Models.Entities
{
    public class Category : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public ICollection<Post> Posts { get; set; } = new List<Post>();

    }
}
