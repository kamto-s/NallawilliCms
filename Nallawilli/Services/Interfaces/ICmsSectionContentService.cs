using Nallawilli.Models.Entities;

namespace Nallawilli.Services.Interfaces
{
    public interface ICmsSectionContentService
    {
        Task<List<CmsSectionContent>> GetBySectionIdOrderedAsync(
            Guid sectionId,
            CancellationToken cancellationToken = default);

        Task<CmsSectionContent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<CmsSectionContent?> GetByIdWithSectionAndPageAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<Dictionary<Guid, int>> GetContentCountsBySectionIdsAsync(
            IReadOnlyCollection<Guid> sectionIds,
            CancellationToken cancellationToken = default);

        Task<CmsSectionContentMutationResult> CreateAsync(
            Guid sectionId,
            string contentKey,
            string? contentValue,
            string inputType,
            int sortOrder,
            string? userId,
            CancellationToken cancellationToken = default);

        Task<CmsSectionContentMutationResult> UpdateAsync(
            Guid id,
            string contentKey,
            string? contentValue,
            string inputType,
            int sortOrder,
            string? userId,
            CancellationToken cancellationToken = default);

        Task<CmsSectionContentDeleteResult> SoftDeleteAsync(
            Guid id,
            string? userId,
            CancellationToken cancellationToken = default);
    }

    public sealed record CmsSectionContentMutationResult(
        bool Success,
        string? Field = null,
        string? Message = null)
    {
        public static CmsSectionContentMutationResult Ok() => new(true);

        public static CmsSectionContentMutationResult Fail(string field, string message) =>
            new(false, field, message);
    }

    public enum CmsSectionContentDeleteResult
    {
        NotFound,
        Deleted
    }
}
