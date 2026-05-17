using Microsoft.AspNetCore.Mvc;
using Nallawilli.Services.Public.Interfaces;
using Nallawilli.ViewModels;

namespace Nallawilli.Controllers
{
    public class PageController : Controller
    {
        private readonly ICmsPublicService _cmsPublic;

        public PageController(ICmsPublicService cmsPublic)
        {
            _cmsPublic = cmsPublic;
        }

        [HttpGet("/page/{slug}")]
        public async Task<IActionResult> Index(string slug, CancellationToken cancellationToken)
        {
            var page = await _cmsPublic.GetPageBySlugAsync(slug, cancellationToken);
            if (page is null)
                return NotFound();

            SetPageViewData(page);
            return View(page);
        }

        internal static void SetPageViewData(Controller controller, CmsPublicPageViewModel page)
        {
            controller.ViewData["Title"] = page.PageTitle ?? page.Title;
            controller.ViewData["MetaDescription"] = page.MetaDescription;
        }

        private void SetPageViewData(CmsPublicPageViewModel page) =>
            SetPageViewData(this, page);
    }
}
