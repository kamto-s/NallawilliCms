using Nallawilli.Models.Entities;

namespace Nallawilli.Services.Interfaces
{
    public interface ICmsSectionService
    {
        Task<List<CmsSection>> GetByPageIdOrderedAsync(Guid pageId, CancellationToken cancellationToken = default);

        Task<CmsSection?> GetByIdWithPageAsync(Guid id, CancellationToken cancellationToken = default);

        Task<CmsSectionMutationResult> CreateAsync(
            Guid pageId,
            string sectionCode,
            string sectionName,
            int sortOrder,
            bool isActive,
            string? userId,
            CancellationToken cancellationToken = default);

        Task<CmsSectionMutationResult> UpdateAsync(
            Guid id,
            string sectionCode,
            string sectionName,
            int sortOrder,
            bool isActive,
            string? userId,
            CancellationToken cancellationToken = default);

        Task<CmsSectionDeleteResult> SoftDeleteAsync(Guid id, string? userId, CancellationToken cancellationToken = default);
    }

    public sealed record CmsSectionMutationResult(bool Success, string? Field = null, string? Message = null)
    {
        public static CmsSectionMutationResult Ok() => new(true);

        public static CmsSectionMutationResult Fail(string field, string message) => new(false, field, message);
    }

    public enum CmsSectionDeleteResult
    {
        NotFound,
        Deleted
    }
}
