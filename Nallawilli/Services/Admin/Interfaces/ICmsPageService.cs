using Nallawilli.Models.Entities;

namespace Nallawilli.Services.Interfaces
{
    public interface ICmsPageService
    {
        Task<List<CmsPage>> GetAllOrderedAsync(CancellationToken cancellationToken = default);

        Task<Dictionary<Guid, int>> GetSectionCountsByPageIdsAsync(
            IReadOnlyCollection<Guid> pageIds,
            CancellationToken cancellationToken = default);

        Task<CmsPage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<CmsPageMutationResult> CreateAsync(
            string title,
            bool isActive,
            bool showInMenu,
            int sortOrder,
            string? metaTitle,
            string? metaDescription,
            string? userId,
            CancellationToken cancellationToken = default);

        Task<CmsPageMutationResult> UpdateAsync(
            Guid id,
            string title,
            bool isActive,
            bool showInMenu,
            int sortOrder,
            string? metaTitle,
            string? metaDescription,
            string? userId,
            CancellationToken cancellationToken = default);

        Task<CmsPageDeleteResult> SoftDeleteAsync(Guid id, string? userId, CancellationToken cancellationToken = default);
    }

    public sealed record CmsPageMutationResult(bool Success, string? Field = null, string? Message = null)
    {
        public static CmsPageMutationResult Ok() => new(true);

        public static CmsPageMutationResult Fail(string field, string message) => new(false, field, message);
    }

    public enum CmsPageDeleteResult
    {
        NotFound,
        Deleted
    }
}
