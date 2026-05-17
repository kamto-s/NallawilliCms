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

            var dbVm = await _manage.GetManageViewModelAsync(id, cancellationToken);
            if (dbVm is null)
                return NotFound();

            MergePostedValuesFromForm(dbVm);

            var updates = await BuildContentUpdatesAsync(dbVm, cancellationToken);
            if (!ModelState.IsValid)
            {
                return View(dbVm);
            }

            var postedImages = GetPostedImageFilesByContentId();
            if (postedImages.Count > 0)
            {
                var imageFieldIds = dbVm.Sections
                    .SelectMany(s => s.Fields)
                    .Where(f => string.Equals(f.InputType, CmsInputTypes.Image, StringComparison.OrdinalIgnoreCase))
                    .Select(f => f.ContentId)
                    .ToHashSet();

                var unmatched = postedImages.Keys.Where(id => !imageFieldIds.Contains(id)).ToList();
                if (unmatched.Count > 0)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Image upload could not be matched. Refresh the page and try again.");
                    return View(dbVm);
                }
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

            MergePostedValuesFromForm(dbVm);
            return View(dbVm);
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
            CmsPageManageViewModel dbVm,
            CancellationToken cancellationToken)
        {
            var updates = new List<CmsContentValueUpdate>();
            var postedImages = GetPostedImageFilesByContentId();

            foreach (var field in dbVm.Sections.SelectMany(s => s.Fields))
            {
                var value = field.ContentValue;

                if (string.Equals(field.InputType, CmsInputTypes.Image, StringComparison.OrdinalIgnoreCase)
                    && postedImages.TryGetValue(field.ContentId, out var file))
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
                    else if (path is not null)
                    {
                        CmsAdminImageStorage.TryDeleteIfManaged(
                            _env,
                            value,
                            CmsAdminImageStorage.WebPrefixSections);
                        value = path;
                    }
                }

                updates.Add(new CmsContentValueUpdate(field.ContentId, value));
            }

            return updates;
        }

        private Dictionary<Guid, IFormFile> GetPostedImageFilesByContentId()
        {
            const string prefix = "imageFile_";
            var map = new Dictionary<Guid, IFormFile>();

            foreach (var file in Request.Form.Files)
            {
                if (!file.Name.StartsWith(prefix, StringComparison.Ordinal)
                    || file.Length == 0)
                    continue;

                if (Guid.TryParse(file.Name[prefix.Length..], out var contentId))
                    map[contentId] = file;
            }

            return map;
        }

        /// <summary>
        /// Read field values directly from the form (reliable with multipart + nested sections).
        /// </summary>
        private void MergePostedValuesFromForm(CmsPageManageViewModel target)
        {
            for (var si = 0; si < target.Sections.Count; si++)
            {
                var section = target.Sections[si];
                for (var fi = 0; fi < section.Fields.Count; fi++)
                {
                    var field = section.Fields[fi];
                    var prefix = $"Sections[{si}].Fields[{fi}]";

                    if (string.Equals(field.InputType, CmsInputTypes.Boolean, StringComparison.OrdinalIgnoreCase))
                    {
                        var raw = Request.Form[$"{prefix}.ContentValue"];
                        field.ContentValue = raw.Count > 0 && raw[^1] == "true" ? "true" : "false";
                        continue;
                    }

                    // Image paths are set only when a new file is uploaded — do not clear from empty hidden input.
                    if (string.Equals(field.InputType, CmsInputTypes.Image, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (Request.Form.TryGetValue($"{prefix}.ContentValue", out var contentValue))
                        field.ContentValue = contentValue.ToString();
                }
            }
        }

        private IActionResult? DenyStructureToolsIfDisabled() =>
            _cmsAdminOptions.ShowPageStructureTools ? null : NotFound();
    }
}
