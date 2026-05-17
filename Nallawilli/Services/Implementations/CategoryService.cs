using Microsoft.EntityFrameworkCore;
using Nallawilli.Data;
using Nallawilli.Helpers;
using Nallawilli.Models.Entities;
using Nallawilli.Services.Interfaces;

namespace Nallawilli.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private const int SlugMaxLength = 300;
        private readonly AppDbContext _db;

        public CategoryService(AppDbContext db)
        {
            _db = db;
        }

        public Task<List<Category>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
        {
            return _db.Categories
                .OrderByDescending(c => c.Id)
                .ToListAsync(cancellationToken);
        }

        public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public Task<bool> HasPostsAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            return _db.Posts.AnyAsync(p => p.CategoryId == categoryId, cancellationToken);
        }

        public async Task<CategoryMutationResult> CreateAsync(string name, string? description, string? userId, CancellationToken cancellationToken = default)
        {
            var slug = await BuildUniqueSlugFromNameAsync(name, excludeCategoryId: null, cancellationToken);
            if (slug.Length > SlugMaxLength)
            {
                return CategoryMutationResult.Fail(
                    "Name",
                    "The URL slug derived from this name is too long. Use a shorter name.");
            }

            var entity = new Category
            {
                Name = name.Trim(),
                Slug = slug,
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _db.Categories.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return CategoryMutationResult.Ok();
        }

        public async Task<CategoryMutationResult> UpdateAsync(Guid id, string name, string? description, string? userId, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (entity is null)
                return CategoryMutationResult.Fail(string.Empty, "Category not found.");

            var slug = await BuildUniqueSlugFromNameAsync(name, excludeCategoryId: id, cancellationToken);
            if (slug.Length > SlugMaxLength)
            {
                return CategoryMutationResult.Fail(
                    "Name",
                    "The URL slug derived from this name is too long. Use a shorter name.");
            }

            entity.Name = name.Trim();
            entity.Slug = slug;
            entity.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;

            await _db.SaveChangesAsync(cancellationToken);

            return CategoryMutationResult.Ok();
        }

        public async Task<CategoryDeleteResult> SoftDeleteAsync(Guid id, string? userId, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (entity is null)
                return CategoryDeleteResult.NotFound;

            if (await HasPostsAsync(id, cancellationToken))
                return CategoryDeleteResult.HasPosts;

            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = userId;

            await _db.SaveChangesAsync(cancellationToken);

            return CategoryDeleteResult.Deleted;
        }

        private static string BaseSlugFromName(string name)
        {
            var slug = SlugHelper.GenerateSlug(name.Trim());
            return string.IsNullOrEmpty(slug) ? "category" : slug;
        }

        private async Task<string> BuildUniqueSlugFromNameAsync(string name, Guid? excludeCategoryId, CancellationToken cancellationToken)
        {
            var baseSlug = BaseSlugFromName(name);
            var candidate = baseSlug;
            var n = 1;

            while (await SlugExistsAsync(candidate, excludeCategoryId, cancellationToken))
            {
                n++;
                var suffix = $"-{n}";
                var maxBase = SlugMaxLength - suffix.Length;
                var truncated = baseSlug.Length > maxBase ? baseSlug[..maxBase].TrimEnd('-') : baseSlug;
                candidate = truncated + suffix;
            }

            return candidate;
        }

        private Task<bool> SlugExistsAsync(string slug, Guid? excludeCategoryId, CancellationToken cancellationToken)
        {
            var q = _db.Categories.IgnoreQueryFilters().AsNoTracking().Where(c => c.Slug == slug);
            if (excludeCategoryId.HasValue)
                q = q.Where(c => c.Id != excludeCategoryId.Value);

            return q.AnyAsync(cancellationToken);
        }
    }
}
