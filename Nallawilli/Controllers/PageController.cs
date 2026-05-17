using Microsoft.AspNetCore.Mvc;

namespace Nallawilli.Controllers
{
    public class PageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
