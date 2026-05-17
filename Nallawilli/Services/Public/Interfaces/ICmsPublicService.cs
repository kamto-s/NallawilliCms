using Nallawilli.ViewModels;

namespace Nallawilli.Services.Public.Interfaces
{
    public interface ICmsPublicService
    {
        Task<CmsPublicPageViewModel?> GetPageBySlugAsync(string slug, CancellationToken cancellationToken = default);
    }
}
