using Microsoft.EntityFrameworkCore;
using Nallawilli.Data;
using Nallawilli.Helpers;
using Nallawilli.Models.Entities;
using Nallawilli.Services.Interfaces;

namespace Nallawilli.Services.Admin.Implementations
{
    public class CmsPageService : ICmsPageService
    {
        private const int SlugMaxLength = 300;
        private readonly AppDbContext _db;

        public CmsPageService(AppDbContext db)
        {
            _db = db;
        }

        public Task<List<CmsPage>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
        {
            return _db.CmsPages
                .AsNoTracking()
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.Title)
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<Guid, int>> GetSectionCountsByPageIdsAsync(
            IReadOnlyCollection<Guid> pageIds,
            CancellationToken cancellationToken = default)
        {
            if (pageIds.Count == 0)
                return new Dictionary<Guid, int>();

            return await _db.CmsSections
                .AsNoTracking()
                .Where(s => pageIds.Contains(s.PageId))
                .GroupBy(s => s.PageId)
                .Select(g => new { PageId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PageId, x => x.Count, cancellationToken);
        }

        public Task<CmsPage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _db.CmsPages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<CmsPageMutationResult> CreateAsync(
            string title,
            bool isActive,
            bool showInMenu,
            int sortOrder,
            string? metaTitle,
            string? metaDescription,
            string? userId,
            CancellationToken cancellationToken = default)
        {
            var slug = await BuildUniqueSlugFromTitleAsync(title, excludePageId: null, cancellationToken);
            if (slug.Length > SlugMaxLength)
            {
                return CmsPageMutationResult.Fail(
                    "Title",
                    "The URL slug derived from this title is too long. Use a shorter title.");
            }

            var entity = new CmsPage
            {
                Title = title.Trim(),
                Slug = slug,
                IsActive = isActive,
                ShowInMenu = showInMenu,
                SortOrder = sortOrder,
                MetaTitle = string.IsNullOrWhiteSpace(metaTitle) ? null : metaTitle.Trim(),
                MetaDescription = string.IsNullOrWhiteSpace(metaDescription) ? null : metaDescription.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _db.CmsPages.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return CmsPageMutationResult.Ok();
        }

        public async Task<CmsPageMutationResult> UpdateAsync(
            Guid id,
            string title,
            bool isActive,
            bool showInMenu,
            int sortOrder,
            string? metaTitle,
            string? metaDescription,
            string? userId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.CmsPages.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (entity is null)
                return CmsPageMutationResult.Fail(string.Empty, "Page not found.");

            var slug = await BuildUniqueSlugFromTitleAsync(title, excludePageId: id, cancellationToken);
            if (slug.Length > SlugMaxLength)
            {
                return CmsPageMutationResult.Fail(
                    "Title",
                    "The URL slug derived from this title is too long. Use a shorter title.");
            }

            entity.Title = title.Trim();
            entity.Slug = slug;
            entity.IsActive = isActive;
            entity.ShowInMenu = showInMenu;
            entity.SortOrder = sortOrder;
            entity.MetaTitle = string.IsNullOrWhiteSpace(metaTitle) ? null : metaTitle.Trim();
            entity.MetaDescription = string.IsNullOrWhiteSpace(metaDescription) ? null : metaDescription.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;

            await _db.SaveChangesAsync(cancellationToken);

            return CmsPageMutationResult.Ok();
        }

        public async Task<CmsPageDeleteResult> SoftDeleteAsync(Guid id, string? userId, CancellationToken cancellationToken = default)
        {
            var entity = await _db.CmsPages
                .Include(p => p.Sections)
                    .ThenInclude(s => s.SectionContents)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (entity is null)
                return CmsPageDeleteResult.NotFound;

            var now = DateTime.UtcNow;
            entity.DeletedAt = now;
            entity.DeletedBy = userId;

            foreach (var section in entity.Sections)
            {
                section.DeletedAt = now;
                section.DeletedBy = userId;

                foreach (var content in section.SectionContents)
                {
                    content.DeletedAt = now;
                    content.DeletedBy = userId;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);

            return CmsPageDeleteResult.Deleted;
        }

        private static string BaseSlugFromTitle(string title)
        {
            var slug = SlugHelper.GenerateSlug(title.Trim());
            return string.IsNullOrEmpty(slug) ? "page" : slug;
        }

        private async Task<string> BuildUniqueSlugFromTitleAsync(string title, Guid? excludePageId, CancellationToken cancellationToken)
        {
            var baseSlug = BaseSlugFromTitle(title);
            var candidate = baseSlug;
            var n = 1;

            while (await SlugExistsAsync(candidate, excludePageId, cancellationToken))
            {
                n++;
                var suffix = $"-{n}";
                var maxBase = SlugMaxLength - suffix.Length;
                var truncated = baseSlug.Length > maxBase ? baseSlug[..maxBase].TrimEnd('-') : baseSlug;
                candidate = truncated + suffix;
            }

            return candidate;
        }

        private Task<bool> SlugExistsAsync(string slug, Guid? excludePageId, CancellationToken cancellationToken)
        {
            var q = _db.CmsPages.IgnoreQueryFilters().AsNoTracking().Where(p => p.Slug == slug);
            if (excludePageId.HasValue)
                q = q.Where(p => p.Id != excludePageId.Value);

            return q.AnyAsync(cancellationToken);
        }
    }
}
