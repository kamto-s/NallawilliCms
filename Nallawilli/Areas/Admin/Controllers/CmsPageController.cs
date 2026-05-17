using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nallawilli.Areas.Admin.ViewModels;
using Nallawilli.Helpers.Admin;
using Nallawilli.Helpers.Common;
using Nallawilli.Options;
using Nallawilli.Services.Interfaces;

namespace Nallawilli.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CmsPageController : Controller
    {
        private readonly ICmsPageService _pages;
        private readonly ICmsManageService _manage;
        private readonly IWebHostEnvironment _env;
        private readonly CmsAdminOptions _cmsAdminOptions;

        public CmsPageController(
            ICmsPageService pages,
            ICmsManageService manage,
            IWebHostEnvironment env,
            IOptions<CmsAdminOptions> cmsAdminOptions)
        {
            _pages = pages;
            _manage = manage;
            _env = env;
            _cmsAdminOptions = cmsAdminOptions.Value;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var list = await _pages.GetAllOrderedAsync(cancellationToken);
            var counts = await _pages.GetSectionCountsByPageIdsAsync(
                list.Select(p => p.Id).ToList(),
                cancellationToken);

            var vm = new CmsPageIndexViewModel
            {
                Pages = list,
                SectionCountsByPageId = counts,
                ShowPageStructureTools = _cmsAdminOptions.ShowPageStructureTools
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Manage(Guid? id, CancellationToken cancellationToken)
        {
            if (id is null)
                return NotFound();

            var vm = await _manage.GetManageViewModelAsync(id.Value, cancellationToken);
            if (vm is null)
                return NotFound();

            ViewBag.ShowPageStructureTools = _cmsAdminOptions.ShowPageStructureTools;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(Guid id, CmsPageManageViewModel model, CancellationToken cancellationToken)
        {
            if (id != model.PageId)
                return BadRequest();

            ViewBag.ShowPageStructureTools = _cmsAdminOptions.ShowPageStructureTools;

            var updates = await BuildContentUpdatesAsync(model, cancellationToken);
            if (!ModelState.IsValid)
            {
                var invalidVm = await _manage.GetManageViewModelAsync(id, cancellationToken);
                if (invalidVm is null)
                    return NotFound();
                MergePostedValues(invalidVm, model);
                return View(invalidVm);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var result = await _manage.SaveContentAsync(id, updates, userId, cancellationToken);

            switch (result)
            {
                case CmsManageSaveResult.PageNotFound:
                    return NotFound();
                case CmsManageSaveResult.InvalidContent:
                    ModelState.AddModelError(string.Empty, "Some content fields could not be saved. Refresh and try again.");
                    break;
                case CmsManageSaveResult.Ok:
                    TempData["Success"] = "Page content saved.";
                    return RedirectToAction(nameof(Manage), new { id });
            }

            var refreshed = await _manage.GetManageViewModelAsync(id, cancellationToken);
            if (refreshed is null)
                return NotFound();

            MergePostedValues(refreshed, model);
            return View(refreshed);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (DenyStructureToolsIfDisabled() is { } denied)
                return denied;

            return View(new CmsPageFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CmsPageFormViewModel model, CancellationToken cancellationToken)
        {
            if (DenyStructureToolsIfDisabled() is { } denied)
                return denied;

            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var result = await _pages.CreateAsync(
                model.Title,
                model.IsActive,
                model.ShowInMenu,
                model.SortOrder,
                model.MetaTitle,
                model.MetaDescription,
                userId,
                cancellationToken);

            if (!result.Success)
            {
                ModelState.AddModelError(result.Field ?? string.Empty, result.Message ?? "Unable to save.");
                return View(model);
            }

            TempData["Success"] = "Page created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id, CancellationToken cancellationToken)
        {
            if (DenyStructureToolsIfDisabled() is { } denied)
                return denied;

            if (id is null)
                return NotFound();

            var entity = await _pages.GetByIdAsync(id.Value, cancellationToken);
            if (entity is null)
                return NotFound();

            var vm = new CmsPageFormViewModel
            {
                Id = entity.Id,
                Title = entity.Title,
                Slug = entity.Slug,
                IsActive = entity.IsActive,
                ShowInMenu = entity.ShowInMenu,
                SortOrder = entity.SortOrder,
                MetaTitle = entity.MetaTitle,
                MetaDescription = entity.MetaDescription
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CmsPageFormViewModel model, CancellationToken cancellationToken)
        {
            if (DenyStructureToolsIfDisabled() is { } denied)
                return denied;

            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var result = await _pages.UpdateAsync(
                id,
                model.Title,
                model.IsActive,
                model.ShowInMenu,
                model.SortOrder,
                model.MetaTitle,
                model.MetaDescription,
                userId,
                cancellationToken);

            if (!result.Success)
            {
                if (string.IsNullOrEmpty(result.Field))
                    return NotFound();

                ModelState.AddModelError(result.Field, result.Message ?? "Unable to save.");
                return View(model);
            }

            TempData["Success"] = "Page updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id, CancellationToken cancellationToken)
        {
            if (DenyStructureToolsIfDisabled() is { } denied)
                return denied;

            if (id is null)
                return NotFound();

            var entity = await _pages.GetByIdAsync(id.Value, cancellationToken);
            if (entity is null)
                return NotFound();

            var sectionCount = (await _pages.GetSectionCountsByPageIdsAsync(
                new[] { id.Value },
                cancellationToken)).GetValueOrDefault(id.Value);

            ViewBag.SectionCount = sectionCount;

            return View(entity);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
        {
            if (DenyStructureToolsIfDisabled() is { } denied)
                return denied;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name;
            var outcome = await _pages.SoftDeleteAsync(id, userId, cancellationToken);

            if (outcome == CmsPageDeleteResult.NotFound)
                return NotFound();

            TempData["Success"] = "Page deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<CmsContentValueUpdate>> BuildContentUpdatesAsync(
            CmsPageManageViewModel model,
            CancellationToken cancellationToken)
        {
            var updates = new List<CmsContentValueUpdate>();

            foreach (var field in model.Sections.SelectMany(s => s.Fields))
            {
                var value = field.ContentValue;

                if (string.Equals(field.InputType, CmsInputTypes.Image, StringComparison.OrdinalIgnoreCase))
                {
                    var file = Request.Form.Files.GetFile($"imageFile_{field.ContentId}");
                    if (file is { Length: > 0 })
                    {
                        var (path, error) = await CmsAdminImageStorage.TrySaveAsync(
                            _env,
                            file,
                            CmsAdminImageStorage.WebPrefixSections,
                            cancellationToken);

                        if (error is not null)
                        {
                            ModelState.AddModelError(
                                string.Empty,
                                $"Image \"{field.ContentKey}\": {error}");
                        }

                        if (path is not null)
                        {
                            CmsAdminImageStorage.TryDeleteIfManaged(
                                _env,
                                value,
                                CmsAdminImageStorage.WebPrefixSections);
                            value = path;
                        }
                    }
                }

                updates.Add(new CmsContentValueUpdate(field.ContentId, value));
            }

            return updates;
        }

        private static void MergePostedValues(CmsPageManageViewModel target, CmsPageManageViewModel posted)
        {
            foreach (var section in target.Sections)
            {
                var postedSection = posted.Sections.FirstOrDefault(s => s.SectionId == section.SectionId);
                if (postedSection is null)
                    continue;

                foreach (var field in section.Fields)
                {
                    var postedField = postedSection.Fields.FirstOrDefault(f => f.ContentId == field.ContentId);
                    if (postedField is not null)
                        field.ContentValue = postedField.ContentValue;
                }
            }
        }

        private IActionResult? DenyStructureToolsIfDisabled() =>
            _cmsAdminOptions.ShowPageStructureTools ? null : NotFound();
    }
}
