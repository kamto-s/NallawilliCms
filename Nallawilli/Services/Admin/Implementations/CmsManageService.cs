using Microsoft.EntityFrameworkCore;
using Nallawilli.Areas.Admin.ViewModels;
using Nallawilli.Data;
using Nallawilli.Helpers;
using Nallawilli.Services.Interfaces;

namespace Nallawilli.Services.Admin.Implementations
{
    public class CmsManageService : ICmsManageService
    {
        private readonly AppDbContext _db;

        public CmsManageService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<CmsPageManageViewModel?> GetManageViewModelAsync(
            Guid pageId,
            CancellationToken cancellationToken = default)
        {
            var page = await _db.CmsPages
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == pageId, cancellationToken);

            if (page is null)
                return null;

            var sections = await _db.CmsSections
                .AsNoTracking()
                .Where(s => s.PageId == pageId)
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.SectionCode)
                .ToListAsync(cancellationToken);

            var sectionIds = sections.Select(s => s.Id).ToList();
            var contents = sectionIds.Count == 0
                ? new List<Models.Entities.CmsSectionContent>()
                : await _db.CmsSectionContents
                    .AsNoTracking()
                    .Where(c => sectionIds.Contains(c.SectionId))
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.ContentKey)
                    .ToListAsync(cancellationToken);

            var contentsBySection = contents
                .GroupBy(c => c.SectionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            return new CmsPageManageViewModel
            {
                PageId = page.Id,
                PageTitle = page.Title,
                PageSlug = page.Slug,
                IsActive = page.IsActive,
                Sections = sections.Select(s => new CmsSectionManageViewModel
                {
                    SectionId = s.Id,
                    SectionCode = s.SectionCode,
                    SectionName = s.SectionName,
                    IsActive = s.IsActive,
                    Fields = (contentsBySection.GetValueOrDefault(s.Id) ?? [])
                        .Select(c => new CmsContentFieldViewModel
                        {
                            ContentId = c.Id,
                            ContentKey = c.ContentKey,
                            ContentValue = c.ContentValue,
                            InputType = c.InputType,
                            SortOrder = c.SortOrder
                        })
                        .ToList()
                }).ToList()
            };
        }

        public async Task<CmsManageSaveResult> SaveContentAsync(
            Guid pageId,
            IReadOnlyList<CmsContentValueUpdate> updates,
            string? userId,
            CancellationToken cancellationToken = default)
        {
            if (!await _db.CmsPages.AnyAsync(p => p.Id == pageId, cancellationToken))
                return CmsManageSaveResult.PageNotFound;

            if (updates.Count == 0)
                return CmsManageSaveResult.Ok;

            var contentIds = updates.Select(u => u.ContentId).Distinct().ToList();

            var entities = await _db.CmsSectionContents
                .Include(c => c.Section)
                .Where(c => contentIds.Contains(c.Id) && c.Section.PageId == pageId)
                .ToListAsync(cancellationToken);

            if (entities.Count != contentIds.Count)
                return CmsManageSaveResult.InvalidContent;

            var now = DateTime.UtcNow;
            var byId = updates.ToDictionary(u => u.ContentId);

            foreach (var entity in entities)
            {
                if (!byId.TryGetValue(entity.Id, out var update))
                    continue;

                entity.ContentValue = ResolveContentValue(entity.InputType, update.ContentValue);
                entity.UpdatedAt = now;
                entity.UpdatedBy = userId;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return CmsManageSaveResult.Ok;
        }

        private static string? ResolveContentValue(string inputType, string? value)
        {
            if (string.Equals(inputType, CmsInputTypes.Color, StringComparison.OrdinalIgnoreCase))
                return CmsColorHelper.ResolveValue(value);

            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
