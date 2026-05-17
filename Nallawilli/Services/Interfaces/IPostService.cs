using Nallawilli.Models.Entities;
using Nallawilli.Models.Enums;

namespace Nallawilli.Services.Interfaces
{
    public interface IPostService
    {
        Task<List<Post>> GetAllOrderedWithCategoryAsync(CancellationToken cancellationToken = default);

        Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<PostMutationResult> CreateAsync(
            string title,
            string content,
            Guid categoryId,
            ContentStatus status,
            DateTime? scheduledPublishAt,
            string? thumbnail,
            string? author,
            string? userId,
            CancellationToken cancellationToken = default);

        Task<PostMutationResult> UpdateAsync(
            Guid id,
            string title,
            string content,
            Guid categoryId,
            ContentStatus status,
            DateTime? scheduledPublishAt,
            string? thumbnail,
            string? userId,
            CancellationToken cancellationToken = default);

        Task<PostDeleteResult> SoftDeleteAsync(Guid id, string? userId, CancellationToken cancellationToken = default);
    }

    public sealed record PostMutationResult(bool Success, string? Field = null, string? Message = null)
    {
        public static PostMutationResult Ok() => new(true);

        public static PostMutationResult Fail(string field, string message) => new(false, field, message);
    }

    public enum PostDeleteResult
    {
        NotFound,
        Deleted
    }
}
