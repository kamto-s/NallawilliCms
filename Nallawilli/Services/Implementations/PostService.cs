using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Nallawilli.Data;
using Nallawilli.Helpers;
using Nallawilli.Models.Entities;
using Nallawilli.Models.Enums;
using Nallawilli.Services.Interfaces;

namespace Nallawilli.Services.Implementations
{
    public class PostService : IPostService
    {
        private const int SlugMaxLength = 500;
        private readonly AppDbContext _db;

        public PostService(AppDbContext db)
        {
            _db = db;
        }

        public Task<List<Post>> GetAllOrderedWithCategoryAsync(CancellationToken cancellationToken = default)
        {
            return _db.Posts
                .AsNoTracking()
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .ToListAsync(cancellationToken);
        }

        public Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _db.Posts
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<PostMutationResult> CreateAsync(
            string title,
            string content,
            Guid categoryId,
            ContentStatus status,
            DateTime? scheduledPublishAt,
            string? thumbnail,
            string? author,
            string? userId,
            CancellationToken cancellationToken = default)
        {
            if (!await CategoryExistsAsync(categoryId, cancellationToken))
                return PostMutationResult.Fail("CategoryId", "Category not found.");

            var scheduledError = ValidateScheduled(status, scheduledPublishAt);
            if (scheduledError is not null)
                return scheduledError;

            content = content.Trim();
            if (IsMeaninglessHtml(content))
                return PostMutationResult.Fail("Content", "Content is required.");

            var slug = await BuildUniqueSlugFromTitleAsync(title, excludePostId: null, cancellationToken);
            if (slug.Length > SlugMaxLength)
            {
                return PostMutationResult.Fail(
                    "Title",
                    "The URL slug derived from this title is too long. Use a shorter title.");
            }

            var entity = new Post
            {
                Title = title.Trim(),
                Slug = slug,
                Content = content,
                CategoryId = categoryId,
                Status = status,
                Thumbnail = string.IsNullOrWhiteSpace(thumbnail) ? null : thumbnail.Trim(),
                Author = string.IsNullOrWhiteSpace(author) ? null : author.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            ApplyPublicationFields(entity, status, scheduledPublishAt);

            _db.Posts.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return PostMutationResult.Ok();
        }

        public async Task<PostMutationResult> UpdateAsync(
            Guid id,
            string title,
            string content,
            Guid categoryId,
            ContentStatus status,
            DateTime? scheduledPublishAt,
            string? thumbnail,
            string? userId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (entity is null)
                return PostMutationResult.Fail(string.Empty, "Post not found.");

            if (!await CategoryExistsAsync(categoryId, cancellationToken))
                return PostMutationResult.Fail("CategoryId", "Category not found.");

            var scheduledError = ValidateScheduled(status, scheduledPublishAt);
            if (scheduledError is not null)
                return scheduledError;

            content = content.Trim();
            if (IsMeaninglessHtml(content))
                return PostMutationResult.Fail("Content", "Content is required.");

            var slug = await BuildUniqueSlugFromTitleAsync(title, excludePostId: id, cancellationToken);
            if (slug.Length > SlugMaxLength)
            {
                return PostMutationResult.Fail(
                    "Title",
                    "The URL slug derived from this title is too long. Use a shorter title.");
            }

            entity.Title = title.Trim();
            entity.Slug = slug;
            entity.Content = content;
            entity.CategoryId = categoryId;
            entity.Status = status;
            entity.Thumbnail = string.IsNullOrWhiteSpace(thumbnail) ? null : thumbnail.Trim();
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = userId;

            ApplyPublicationFields(entity, status, scheduledPublishAt);

            await _db.SaveChangesAsync(cancellationToken);

            return PostMutationResult.Ok();
        }

        public async Task<PostDeleteResult> SoftDeleteAsync(Guid id, string? userId, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (entity is null)
                return PostDeleteResult.NotFound;

            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = userId;

            await _db.SaveChangesAsync(cancellationToken);

            return PostDeleteResult.Deleted;
        }

        private static PostMutationResult? ValidateScheduled(ContentStatus status, DateTime? scheduledPublishAt)
        {
            if (status != ContentStatus.Scheduled)
                return null;

            if (!scheduledPublishAt.HasValue)
            {
                return PostMutationResult.Fail(
                    "ScheduledPublishAt",
                    "Schedule date and time are required when status is Scheduled.");
            }

            return null;
        }

        private static void ApplyPublicationFields(Post entity, ContentStatus status, DateTime? scheduledPublishAt)
        {
            if (status == ContentStatus.Scheduled)
            {
                entity.ScheduledPublishAt = scheduledPublishAt;
                entity.PublishedAt = null;
            }
            else if (status == ContentStatus.Published)
            {
                entity.ScheduledPublishAt = null;
                entity.PublishedAt ??= DateTime.UtcNow;
            }
            else
            {
                entity.ScheduledPublishAt = null;
                entity.PublishedAt = null;
            }
        }

        private Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken)
        {
            return _db.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken);
        }

        private static string BaseSlugFromTitle(string title)
        {
            var slug = SlugHelper.GenerateSlug(title.Trim());
            return string.IsNullOrEmpty(slug) ? "post" : slug;
        }

        private async Task<string> BuildUniqueSlugFromTitleAsync(string title, Guid? excludePostId, CancellationToken cancellationToken)
        {
            var baseSlug = BaseSlugFromTitle(title);
            var candidate = baseSlug;
            var n = 1;

            while (await SlugExistsAsync(candidate, excludePostId, cancellationToken))
            {
                n++;
                var suffix = $"-{n}";
                var maxBase = SlugMaxLength - suffix.Length;
                var truncated = baseSlug.Length > maxBase ? baseSlug[..maxBase].TrimEnd('-') : baseSlug;
                candidate = truncated + suffix;
            }

            return candidate;
        }

        private static bool IsMeaninglessHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return true;
            if (html.Contains("<img", StringComparison.OrdinalIgnoreCase))
                return false;
            var text = Regex.Replace(html, @"<[^>]+>", string.Empty);
            return string.IsNullOrWhiteSpace(text);
        }

        private Task<bool> SlugExistsAsync(string slug, Guid? excludePostId, CancellationToken cancellationToken)
        {
            var q = _db.Posts.IgnoreQueryFilters().AsNoTracking().Where(p => p.Slug == slug);
            if (excludePostId.HasValue)
                q = q.Where(p => p.Id != excludePostId.Value);

            return q.AnyAsync(cancellationToken);
        }
    }
}
