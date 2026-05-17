using Microsoft.EntityFrameworkCore;
using Nallawilli.Data;
using Nallawilli.Helpers;
using Nallawilli.Models.Entities;
using Nallawilli.Services.Interfaces;

namespace Nallawilli.Services.Implementations
{
    public class CmsSectionContentService : ICmsSectionContentService
    {
        private readonly AppDbContext _db;

        public CmsSectionContentService(AppDbContext db)
        {
            _db = db;
        }

        public Task<List<CmsSectionContent>> GetBySectionIdOrderedAsync(
            Guid sectionId,
            CancellationToken cancellationToken = default)
        {
            return _db.CmsSectionContents
                .AsNoTracking()
                .Where(c => c.SectionId == sectionId)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.ContentKey)
                .ToListAsync(cancellationToken);
        }

        public Task<CmsSectionContent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _db.CmsSectionContents
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public Task<CmsSectionContent?> GetByIdWithSectionAndPageAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return _db.CmsSectionContents
                .AsNoTracking()
                .Include(c => c.Section)
                    .ThenInclude(s => s.Page)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Dictionary<Guid, int>> GetContentCountsBySectionIdsAsync(
            IReadOnlyCollection<Guid> sectionIds,
            CancellationToken cancellationToken = default)
        {
            if (sectionIds.Count == 0)
                return new Dictionary<Guid, int>();

            return await _db.CmsSectionContents
                .AsNoTracking()
                .Where(c => sectionIds.Contains(c.SectionId))
                .GroupBy(c => c.SectionId)
                .Select(g => new { SectionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SectionId, x => x.Count, cancellationToken);
        }

        public async Task<CmsSectionContentMutationResult> CreateAsync(
            Guid sectionId,
            string contentKey,
            string? contentValue,
            string inputType,
            int sortOrder,
            string? userId,
            CancellationToken cancellationToken = default)
        {
            if (!await SectionExistsAsync(sectionId, cancellationToken))
                return CmsSectionContentMutationResult.Fail("SectionId", "Section not found.");

            if (!CmsInputTypes.IsValid(inputType))
            {
                return CmsSectionContentMutationResult.Fail(
                    "InputType",
                    "Invalid input type.");
            }

            var key = contentKey.Trim().ToLowerInvariant();
            if (await ContentKeyExistsOnSectionAsync(sectionId, key, excludeContentId: null, cancellationToken))
            {
                return CmsSectionContentMutationResult.Fail(
                    "ContentKey",
                    "A content item with this key already exists in this section.");
            }

            var normalizedType = inputType.Trim().ToLowerInvariant();
            var entity = new CmsSectionContent
            {
                SectionId = sectionId,
                ContentKey = key,
                ContentValue = ResolveContentValue(normalizedType, contentValue),
                InputType = inputType.Trim().ToLowerInvariant(),
                SortOrder = sortOrder,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _db.CmsSectionContents.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return CmsSectionContentMutationResult.Ok();
        }

        public async Task<CmsSectionContentMutationResult> UpdateAsync(
            Guid id,
            string contentKey,
            string? contentValue,
            string inputType,
            int sortOrder,
            string? userId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.CmsSectionContents
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (entity is null)
                return CmsSectionContentMutationResult.Fail(string.Empty, "Content not found.");

            if (!CmsInputTypes.IsValid(inputType))
            {
                return CmsSectionContentMutationResult.Fail(
                    "InputType",
                    "Invalid input type.");
            }

            var key = contentKey.Trim().ToLowerInvariant();
            if (await ContentKeyExistsOnSectionAsync(
                    entity.SectionId,
                    key,
                    excludeContentId: id,
                    cancellationToken))
            {
                return CmsSectionContentMutationResult.Fail(
                    "ContentKey",
                    "A content item with this key already exists in this section.");
            }

            entity.ContentKey = key;
            var normalizedType = inputType.Trim().ToLowerInvariant();
            entity.ContentValue = ResolveContentValue(normalizedType, contentValue);
            entity.InputType = normalizedType;
            entity.SortOrder = sortOrder;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;

            await _db.SaveChangesAsync(cancellationToken);

            return CmsSectionContentMutationResult.Ok();
        }

        public async Task<CmsSectionContentDeleteResult> SoftDeleteAsync(
            Guid id,
            string? userId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.CmsSectionContents
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (entity is null)
                return CmsSectionContentDeleteResult.NotFound;

            var now = DateTime.UtcNow;
            entity.DeletedAt = now;
            entity.DeletedBy = userId;

            await _db.SaveChangesAsync(cancellationToken);

            return CmsSectionContentDeleteResult.Deleted;
        }

        private Task<bool> SectionExistsAsync(Guid sectionId, CancellationToken cancellationToken)
        {
            return _db.CmsSections.AnyAsync(s => s.Id == sectionId, cancellationToken);
        }

        private Task<bool> ContentKeyExistsOnSectionAsync(
            Guid sectionId,
            string contentKey,
            Guid? excludeContentId,
            CancellationToken cancellationToken)
        {
            var q = _db.CmsSectionContents
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => c.SectionId == sectionId && c.ContentKey == contentKey);

            if (excludeContentId.HasValue)
                q = q.Where(c => c.Id != excludeContentId.Value);

            return q.AnyAsync(cancellationToken);
        }

        private static string? ResolveContentValue(string inputType, string? value)
        {
            if (string.Equals(inputType, CmsInputTypes.Color, StringComparison.OrdinalIgnoreCase))
                return CmsColorHelper.ResolveValue(value);

            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
