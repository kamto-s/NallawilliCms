using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nallawilli.Areas.Admin.ViewModels;
using Nallawilli.Services.Interfaces;

namespace Nallawilli.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CmsSectionController : Controller
    {
        private readonly ICmsPageService _pages;
        private readonly ICmsSectionService _sections;
        private readonly ICmsSectionContentService _contents;

        public CmsSectionController(
            ICmsPageService pages,
            ICmsSectionService sections,
            ICmsSectionContentService contents)
        {
            _pages = pages;
            _sections = sections;
            _contents = contents;
        }

        public async Task<IActionResult> Index(Guid pageId, CancellationToken cancellationToken)
        {
            var page = await _pages.GetByIdAsync(pageId, cancellationToken);
            if (page is null)
                return NotFound();

            var sections = await _sections.GetByPageIdOrderedAsync(pageId, cancellationToken);
            var contentCounts = await _contents.GetContentCountsBySectionIdsAsync(
                sections.Select(s => s.Id).ToList(),
                cancellationToken);

            var vm = new CmsSectionListViewModel
            {
                Page = page,
                Sections = sections,
                ContentCountsBySectionId = contentCounts
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid pageId, CancellationToken cancellationToken)
        {
            var page = await _pages.GetByIdAsync(pageId, cancellationToken);
            if (page is null)
                return NotFound();

            return View(new CmsSectionFormViewModel
            {
                PageId = pageId,
                PageTitle = page.Title
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid pageId, CmsSectionFormViewModel model, CancellationToken cancellationToken)
        {
            if (pageId != model.PageId)
                return BadRequest();

            var page = await _pages.GetByIdAsync(pageId, cancellationToken);
            if (page is null)
                return NotFound();

            model.PageTitle = page.Title;
            NormalizeSectionFormModel(model);

            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var result = await _sections.CreateAsync(
                pageId,
                model.SectionCode,
                model.SectionName,
                model.SortOrder,
                model.IsActive,
                userId,
                cancellationToken);

            if (!result.Success)
            {
                ModelState.AddModelError(result.Field ?? string.Empty, result.Message ?? "Unable to save.");
                return View(model);
            }

            TempData["Success"] = "Section created.";
            return RedirectToAction(nameof(Index), new { pageId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id, CancellationToken cancellationToken)
        {
            if (id is null)
                return NotFound();

            var entity = await _sections.GetByIdWithPageAsync(id.Value, cancellationToken);
            if (entity is null)
                return NotFound();

            var vm = new CmsSectionFormViewModel
            {
                Id = entity.Id,
                PageId = entity.PageId,
                PageTitle = entity.Page.Title,
                SectionCode = entity.SectionCode,
                SectionName = entity.SectionName,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CmsSectionFormViewModel model, CancellationToken cancellationToken)
        {
            if (id != model.Id)
                return BadRequest();

            var entity = await _sections.GetByIdWithPageAsync(id, cancellationToken);
            if (entity is null)
                return NotFound();

            model.PageId = entity.PageId;
            model.PageTitle = entity.Page.Title;
            NormalizeSectionFormModel(model);

            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var result = await _sections.UpdateAsync(
                id,
                model.SectionCode,
                model.SectionName,
                model.SortOrder,
                model.IsActive,
                userId,
                cancellationToken);

            if (!result.Success)
            {
                if (string.IsNullOrEmpty(result.Field))
                    return NotFound();

                ModelState.AddModelError(result.Field, result.Message ?? "Unable to save.");
                return View(model);
            }

            TempData["Success"] = "Section updated.";
            return RedirectToAction(nameof(Index), new { pageId = model.PageId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id, CancellationToken cancellationToken)
        {
            if (id is null)
                return NotFound();

            var entity = await _sections.GetByIdWithPageAsync(id.Value, cancellationToken);
            if (entity is null)
                return NotFound();

            var contentCount = (await _contents.GetContentCountsBySectionIdsAsync(
                new[] { id.Value },
                cancellationToken)).GetValueOrDefault(id.Value);

            ViewBag.ContentCount = contentCount;
            ViewBag.PageId = entity.PageId;

            return View(entity);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _sections.GetByIdWithPageAsync(id, cancellationToken);
            if (entity is null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var outcome = await _sections.SoftDeleteAsync(id, userId, cancellationToken);

            if (outcome == CmsSectionDeleteResult.NotFound)
                return NotFound();

            TempData["Success"] = "Section deleted.";
            return RedirectToAction(nameof(Index), new { pageId = entity.PageId });
        }

        private void NormalizeSectionFormModel(CmsSectionFormViewModel model)
        {
            model.SectionCode = (model.SectionCode ?? string.Empty).Trim().ToLowerInvariant();
            model.SectionName = (model.SectionName ?? string.Empty).Trim();

            ModelState.Remove(nameof(model.SectionCode));
            ModelState.Remove(nameof(model.SectionName));
            TryValidateModel(model);
        }
    }
}
