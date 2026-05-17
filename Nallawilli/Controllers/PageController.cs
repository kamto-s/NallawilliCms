using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nallawilli.Helpers.Public;
using Nallawilli.Models;
using Nallawilli.Services.Public.Interfaces;
using Nallawilli.ViewModels.Public;

namespace Nallawilli.Controllers
{
    public class PageController : Controller
    {
        private readonly ICmsPublicService _cmsPublic;
        
        public PageController(ICmsPublicService cmsPublic)
        {
            _cmsPublic = cmsPublic;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default) =>
            await RenderPageAsync(CmsPublicDefaults.HomePageSlug, cancellationToken);

        public async Task<IActionResult> Page(string slug, CancellationToken cancellationToken = default) =>
            await RenderPageAsync(slug, cancellationToken);

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        private async Task<IActionResult> RenderPageAsync(string slug, CancellationToken cancellationToken)
        {
            var page = await _cmsPublic.GetPageBySlugAsync(slug, cancellationToken);
            if (page is null)
                return NotFound();

            ViewData["Title"] = page.MetaTitle ?? page.Title;
            ViewData["MetaDescription"] = page.MetaDescription;
            return View("Index", page);
        }
    }
}
