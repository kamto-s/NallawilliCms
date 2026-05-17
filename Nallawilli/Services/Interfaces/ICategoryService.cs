using Nallawilli.Models.Entities;

namespace Nallawilli.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllOrderedAsync(CancellationToken cancellationToken = default);

        Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<bool> HasPostsAsync(Guid categoryId, CancellationToken cancellationToken = default);

        Task<CategoryMutationResult> CreateAsync(string name, string? description, string? userId, CancellationToken cancellationToken = default);

        Task<CategoryMutationResult> UpdateAsync(Guid id, string name, string? description, string? userId, CancellationToken cancellationToken = default);

        Task<CategoryDeleteResult> SoftDeleteAsync(Guid id, string? userId, CancellationToken cancellationToken = default);
    }

    public sealed record CategoryMutationResult(bool Success, string? Field = null, string? Message = null)
    {
        public static CategoryMutationResult Ok() => new(true);

        public static CategoryMutationResult Fail(string field, string message) => new(false, field, message);
    }

    public enum CategoryDeleteResult
    {
        NotFound,
        HasPosts,
        Deleted
    }
}
