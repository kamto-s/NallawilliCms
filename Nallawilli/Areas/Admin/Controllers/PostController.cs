using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nallawilli.Areas.Admin.ViewModels;
using Nallawilli.Services.Interfaces;

namespace Nallawilli.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class PostController : Controller
    {
        private const long MaxThumbnailBytes = 5 * 1024 * 1024;
        private static readonly string[] AllowedThumbnailExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

        private readonly IPostService _posts;
        private readonly ICategoryService _categories;
        private readonly IWebHostEnvironment _env;

        public PostController(IPostService posts, ICategoryService categories, IWebHostEnvironment env)
        {
            _posts = posts;
            _categories = categories;
            _env = env;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var list = await _posts.GetAllOrderedWithCategoryAsync(cancellationToken);
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            var vm = new PostFormViewModel();
            await FillCategoryOptionsAsync(vm, cancellationToken);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxThumbnailBytes + 1024 * 1024)]
        public async Task<IActionResult> Create(PostFormViewModel model, CancellationToken cancellationToken)
        {
            await FillCategoryOptionsAsync(model, cancellationToken);

            if (!ModelState.IsValid)
                return View(model);

            if (model.CategoryId == Guid.Empty)
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Please select a category.");
                return View(model);
            }

            var (thumbPath, thumbError) = await TrySaveThumbnailAsync(model.ThumbnailFile, cancellationToken);
            if (thumbError is not null)
            {
                ModelState.AddModelError(nameof(model.ThumbnailFile), thumbError);
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var author = ResolveAuthorDisplayName(User);
            var result = await _posts.CreateAsync(
                model.Title,
                model.Content,
                model.CategoryId,
                model.Status,
                model.ScheduledPublishAt,
                thumbPath,
                author,
                userId,
                cancellationToken);

            if (!result.Success)
            {
                if (!string.IsNullOrEmpty(thumbPath))
                    TryDeletePhysicalFile(thumbPath);
                ModelState.AddModelError(result.Field ?? string.Empty, result.Message ?? "Unable to save.");
                return View(model);
            }

            TempData["Success"] = "Post created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id, CancellationToken cancellationToken)
        {
            if (id is null)
                return NotFound();

            var entity = await _posts.GetByIdAsync(id.Value, cancellationToken);
            if (entity is null)
                return NotFound();

            var vm = new PostFormViewModel
            {
                Id = entity.Id,
                Title = entity.Title,
                Content = entity.Content,
                CategoryId = entity.CategoryId,
                Status = entity.Status,
                ScheduledPublishAt = entity.ScheduledPublishAt,
                Thumbnail = entity.Thumbnail
            };

            await FillCategoryOptionsAsync(vm, cancellationToken);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxThumbnailBytes + 1024 * 1024)]
        public async Task<IActionResult> Edit(Guid id, PostFormViewModel model, CancellationToken cancellationToken)
        {
            if (id != model.Id)
                return BadRequest();

            await FillCategoryOptionsAsync(model, cancellationToken);

            if (!ModelState.IsValid)
                return View(model);

            if (model.CategoryId == Guid.Empty)
            {
                ModelState.AddModelError(nameof(model.CategoryId), "Please select a category.");
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var previousThumb = model.Thumbnail;

            var (newThumbPath, thumbError) = await TrySaveThumbnailAsync(model.ThumbnailFile, cancellationToken);
            if (thumbError is not null)
            {
                ModelState.AddModelError(nameof(model.ThumbnailFile), thumbError);
                return View(model);
            }

            var thumbPath = newThumbPath ?? model.Thumbnail;
            if (string.IsNullOrWhiteSpace(thumbPath))
                thumbPath = null;

            var result = await _posts.UpdateAsync(
                id,
                model.Title,
                model.Content,
                model.CategoryId,
                model.Status,
                model.ScheduledPublishAt,
                thumbPath,
                userId,
                cancellationToken);

            if (!result.Success)
            {
                if (!string.IsNullOrEmpty(newThumbPath))
                    TryDeletePhysicalFile(newThumbPath);
                if (string.IsNullOrEmpty(result.Field))
                    return NotFound();

                ModelState.AddModelError(result.Field, result.Message ?? "Unable to save.");
                return View(model);
            }

            if (!string.IsNullOrEmpty(newThumbPath) && !string.Equals(previousThumb, newThumbPath, StringComparison.Ordinal))
                TryDeletePhysicalFile(previousThumb);

            TempData["Success"] = "Post updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id, CancellationToken cancellationToken)
        {
            if (id is null)
                return NotFound();

            var entity = await _posts.GetByIdAsync(id.Value, cancellationToken);
            if (entity is null)
                return NotFound();

            return View(entity);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var outcome = await _posts.SoftDeleteAsync(id, userId, cancellationToken);

            if (outcome == PostDeleteResult.NotFound)
                return NotFound();

            TempData["Success"] = "Post deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task FillCategoryOptionsAsync(PostFormViewModel vm, CancellationToken cancellationToken)
        {
            var cats = await _categories.GetAllOrderedAsync(cancellationToken);
            vm.CategoryOptions = cats
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name, Selected = c.Id == vm.CategoryId })
                .ToList();
        }

        private static string? ResolveAuthorDisplayName(ClaimsPrincipal user)
        {
            return user.Identity?.Name
                ?? user.FindFirstValue(ClaimTypes.Email)
                ?? user.FindFirstValue(ClaimTypes.Name);
        }

        private async Task<(string? path, string? error)> TrySaveThumbnailAsync(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return (null, null);

            if (file.Length > MaxThumbnailBytes)
                return (null, "Image must be 5 MB or smaller.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedThumbnailExtensions.Contains(ext))
                return (null, "Allowed image types: JPG, PNG, GIF, WebP.");

            var dir = Path.Combine(_env.WebRootPath, "uploads", "posts");
            Directory.CreateDirectory(dir);

            var name = $"{Guid.NewGuid():N}{ext}";
            var physical = Path.Combine(dir, name);

            await using (var stream = System.IO.File.Create(physical))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var webPath = "/uploads/posts/" + name;
            return (webPath, null);
        }

        private void TryDeletePhysicalFile(string? webPath)
        {
            if (string.IsNullOrEmpty(webPath) || !webPath.StartsWith("/uploads/posts/", StringComparison.OrdinalIgnoreCase))
                return;

            var relative = webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physical = Path.Combine(_env.WebRootPath, relative);
            if (System.IO.File.Exists(physical))
                System.IO.File.Delete(physical);
        }
    }
}
