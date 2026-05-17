using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nallawilli.Helpers;
using Nallawilli.Models;
using Nallawilli.Services.Public.Interfaces;

namespace Nallawilli.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICmsPublicService _cmsPublic;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ICmsPublicService cmsPublic, ILogger<HomeController> logger)
        {
            _cmsPublic = cmsPublic;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var page = await _cmsPublic.GetPageBySlugAsync(CmsPublicDefaults.HomePageSlug, cancellationToken);
            if (page is not null)
            {
                PageController.SetPageViewData(this, page);
                return View("~/Views/Page/Index.cshtml", page);
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
