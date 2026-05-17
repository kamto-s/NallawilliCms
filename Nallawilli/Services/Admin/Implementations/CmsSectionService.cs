using Microsoft.EntityFrameworkCore;
using Nallawilli.Data;
using Nallawilli.Models.Entities;
using Nallawilli.Services.Interfaces;

namespace Nallawilli.Services.Admin.Implementations
{
    public class CmsSectionService : ICmsSectionService
    {
        private readonly AppDbContext _db;

        public CmsSectionService(AppDbContext db)
        {
            _db = db;
        }

        public Task<List<CmsSection>> GetByPageIdOrderedAsync(Guid pageId, CancellationToken cancellationToken = default)
        {
            return _db.CmsSections
                .AsNoTracking()
                .Where(s => s.PageId == pageId)
                .OrderBy(s => s.SortOrder)
                .ThenByDescending(s => s.Id)
                .ToListAsync(cancellationToken);
        }

        public Task<CmsSection?> GetByIdWithPageAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _db.CmsSections
                .AsNoTracking()
                .Include(s => s.Page)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<CmsSectionMutationResult> CreateAsync(
            Guid pageId,
            string sectionCode,
            string sectionName,
            int sortOrder,
            bool isActive,
            string? userId,
            CancellationToken cancellationToken = default)
        {
            if (!await PageExistsAsync(pageId, cancellationToken))
                return CmsSectionMutationResult.Fail("PageId", "CMS page not found.");

            var code = sectionCode.Trim().ToLowerInvariant();

            var entity = new CmsSection
            {
                PageId = pageId,
                SectionCode = code,
                SectionName = sectionName.Trim(),
                SortOrder = sortOrder,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _db.CmsSections.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return CmsSectionMutationResult.Ok();
        }

        public async Task<CmsSectionMutationResult> UpdateAsync(
            Guid id,
            string sectionCode,
            string sectionName,
            int sortOrder,
            bool isActive,
            string? userId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.CmsSections.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (entity is null)
                return CmsSectionMutationResult.Fail(string.Empty, "Section not found.");

            var code = sectionCode.Trim().ToLowerInvariant();

            entity.SectionCode = code;
            entity.SectionName = sectionName.Trim();
            entity.SortOrder = sortOrder;
            entity.IsActive = isActive;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;

            await _db.SaveChangesAsync(cancellationToken);

            return CmsSectionMutationResult.Ok();
        }

        public async Task<CmsSectionDeleteResult> SoftDeleteAsync(Guid id, string? userId, CancellationToken cancellationToken = default)
        {
            var entity = await _db.CmsSections
                .Include(s => s.SectionContents)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (entity is null)
                return CmsSectionDeleteResult.NotFound;

            var now = DateTime.UtcNow;
            entity.DeletedAt = now;
            entity.DeletedBy = userId;

            foreach (var content in entity.SectionContents)
            {
                content.DeletedAt = now;
                content.DeletedBy = userId;
            }

            await _db.SaveChangesAsync(cancellationToken);

            return CmsSectionDeleteResult.Deleted;
        }

        private Task<bool> PageExistsAsync(Guid pageId, CancellationToken cancellationToken)
        {
            return _db.CmsPages.AnyAsync(p => p.Id == pageId, cancellationToken);
        }
    }
}
