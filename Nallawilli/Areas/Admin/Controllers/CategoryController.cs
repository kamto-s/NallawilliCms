using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nallawilli.Areas.Admin.ViewModels;
using Nallawilli.Services.Interfaces;

namespace Nallawilli.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categories;

        public CategoryController(ICategoryService categories)
        {
            _categories = categories;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var list = await _categories.GetAllOrderedAsync(cancellationToken);
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CategoryFormViewModel());   
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var result = await _categories.CreateAsync(model.Name, model.Description, userId, cancellationToken);

            if (!result.Success)
            {
                ModelState.AddModelError(result.Field ?? string.Empty, result.Message ?? "Unable to save.");
                return View(model);
            }

            TempData["Success"] = "Category created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id, CancellationToken cancellationToken)
        {
            if (id is null)
                return NotFound();

            var entity = await _categories.GetByIdAsync(id.Value, cancellationToken);
            if (entity is null)
                return NotFound();

            var vm = new CategoryFormViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CategoryFormViewModel model, CancellationToken cancellationToken)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var result = await _categories.UpdateAsync(id, model.Name, model.Description, userId, cancellationToken);

            if (!result.Success)
            {
                if (string.IsNullOrEmpty(result.Field))
                {
                    return NotFound();
                }

                ModelState.AddModelError(result.Field, result.Message ?? "Unable to save.");
                return View(model);
            }

            TempData["Success"] = "Category updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id, CancellationToken cancellationToken)
        {
            if (id is null)
                return NotFound();

            var entity = await _categories.GetByIdAsync(id.Value, cancellationToken);
            if (entity is null)
                return NotFound();

            ViewBag.HasPosts = await _categories.HasPostsAsync(id.Value, cancellationToken);

            return View(entity);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var outcome = await _categories.SoftDeleteAsync(id, userId, cancellationToken);

            switch (outcome)
            {
                case CategoryDeleteResult.NotFound:
                    return NotFound();
                case CategoryDeleteResult.HasPosts:
                    TempData["Error"] = "Cannot delete this category while posts still use it. Reassign or remove those posts first.";
                    return RedirectToAction(nameof(Delete), new { id });
                case CategoryDeleteResult.Deleted:
                    TempData["Success"] = "Category deleted.";
                    return RedirectToAction(nameof(Index));
                default:
                    return RedirectToAction(nameof(Index));
            }
        }
    }
}
