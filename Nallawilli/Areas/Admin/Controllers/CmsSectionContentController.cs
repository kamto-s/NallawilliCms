using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nallawilli.Areas.Admin.ViewModels;
using Nallawilli.Helpers;
using Nallawilli.Services.Interfaces;

namespace Nallawilli.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CmsSectionContentController : Controller
    {
        private readonly ICmsSectionService _sections;
        private readonly ICmsSectionContentService _contents;

        public CmsSectionContentController(
            ICmsSectionService sections,
            ICmsSectionContentService contents)
        {
            _sections = sections;
            _contents = contents;
        }

        public async Task<IActionResult> Index(Guid sectionId, CancellationToken cancellationToken)
        {
            var section = await _sections.GetByIdWithPageAsync(sectionId, cancellationToken);
            if (section is null)
                return NotFound();

            var list = await _contents.GetBySectionIdOrderedAsync(sectionId, cancellationToken);

            var vm = new CmsSectionContentListViewModel
            {
                Page = section.Page,
                Section = section,
                Contents = list
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid sectionId, CancellationToken cancellationToken)
        {
            var section = await _sections.GetByIdWithPageAsync(sectionId, cancellationToken);
            if (section is null)
                return NotFound();

            ViewBag.InputTypes = CmsInputTypes.All;

            return View(new CmsSectionContentFormViewModel
            {
                SectionId = sectionId,
                PageId = section.PageId,
                PageTitle = section.Page.Title,
                SectionName = section.SectionName,
                SectionCode = section.SectionCode,
                InputType = CmsInputTypes.Text
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Guid sectionId,
            CmsSectionContentFormViewModel model,
            CancellationToken cancellationToken)
        {
            if (sectionId != model.SectionId)
                return BadRequest();

            var section = await _sections.GetByIdWithPageAsync(sectionId, cancellationToken);
            if (section is null)
                return NotFound();

            model.PageId = section.PageId;
            model.PageTitle = section.Page.Title;
            model.SectionName = section.SectionName;
            model.SectionCode = section.SectionCode;
            ViewBag.InputTypes = CmsInputTypes.All;

            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var result = await _contents.CreateAsync(
                sectionId,
                model.ContentKey,
                model.ContentValue,
                model.InputType,
                model.SortOrder,
                userId,
                cancellationToken);

            if (!result.Success)
            {
                ModelState.AddModelError(result.Field ?? string.Empty, result.Message ?? "Unable to save.");
                ViewBag.InputTypes = CmsInputTypes.All;
                return View(model);
            }

            TempData["Success"] = "Content created.";
            return RedirectToAction(nameof(Index), new { sectionId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id, CancellationToken cancellationToken)
        {
            if (id is null)
                return NotFound();

            var entity = await _contents.GetByIdWithSectionAndPageAsync(id.Value, cancellationToken);
            if (entity is null)
                return NotFound();

            ViewBag.InputTypes = CmsInputTypes.All;

            return View(new CmsSectionContentFormViewModel
            {
                Id = entity.Id,
                SectionId = entity.SectionId,
                PageId = entity.Section.PageId,
                PageTitle = entity.Section.Page.Title,
                SectionName = entity.Section.SectionName,
                SectionCode = entity.Section.SectionCode,
                ContentKey = entity.ContentKey,
                ContentValue = entity.ContentValue,
                InputType = entity.InputType,
                SortOrder = entity.SortOrder
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            CmsSectionContentFormViewModel model,
            CancellationToken cancellationToken)
        {
            if (id != model.Id)
                return BadRequest();

            var entity = await _contents.GetByIdWithSectionAndPageAsync(id, cancellationToken);
            if (entity is null)
                return NotFound();

            model.SectionId = entity.SectionId;
            model.PageId = entity.Section.PageId;
            model.PageTitle = entity.Section.Page.Title;
            model.SectionName = entity.Section.SectionName;
            model.SectionCode = entity.Section.SectionCode;
            ViewBag.InputTypes = CmsInputTypes.All;

            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var result = await _contents.UpdateAsync(
                id,
                model.ContentKey,
                model.ContentValue,
                model.InputType,
                model.SortOrder,
                userId,
                cancellationToken);

            if (!result.Success)
            {
                if (string.IsNullOrEmpty(result.Field))
                    return NotFound();

                ModelState.AddModelError(result.Field, result.Message ?? "Unable to save.");
                return View(model);
            }

            TempData["Success"] = "Content updated.";
            return RedirectToAction(nameof(Index), new { sectionId = model.SectionId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id, CancellationToken cancellationToken)
        {
            if (id is null)
                return NotFound();

            var entity = await _contents.GetByIdWithSectionAndPageAsync(id.Value, cancellationToken);
            if (entity is null)
                return NotFound();

            ViewBag.PageId = entity.Section.PageId;
            ViewBag.SectionId = entity.SectionId;

            return View(entity);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _contents.GetByIdWithSectionAndPageAsync(id, cancellationToken);
            if (entity is null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var outcome = await _contents.SoftDeleteAsync(id, userId, cancellationToken);

            if (outcome == CmsSectionContentDeleteResult.NotFound)
                return NotFound();

            TempData["Success"] = "Content deleted.";
            return RedirectToAction(nameof(Index), new { sectionId = entity.SectionId });
        }
    }
}
