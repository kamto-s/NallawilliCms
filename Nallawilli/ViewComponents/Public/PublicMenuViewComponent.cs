using Microsoft.AspNetCore.Mvc;
using Nallawilli.Services.Public.Interfaces;

namespace Nallawilli.ViewComponents.Public
{
    public class PublicMenuViewComponent : ViewComponent
    {
        private readonly ICmsPublicService _cmsPublic;

        public PublicMenuViewComponent(ICmsPublicService cmsPublic)
        {
            _cmsPublic = cmsPublic;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await _cmsPublic.GetMenuPagesAsync();
            return View(items);
        }
    }
}
