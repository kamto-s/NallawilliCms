using Nallawilli.ViewModels.Public;

namespace Nallawilli.Services.Public.Interfaces
{
    public interface ICmsPublicService
    {
        Task<CmsPublicPageViewModel?> GetPageBySlugAsync(string slug, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CmsPublicMenuItemViewModel>> GetMenuPagesAsync(CancellationToken ct = default);
    }
}
