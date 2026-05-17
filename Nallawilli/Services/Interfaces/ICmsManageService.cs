using Nallawilli.Areas.Admin.ViewModels;

namespace Nallawilli.Services.Interfaces
{
    public interface ICmsManageService
    {
        Task<CmsPageManageViewModel?> GetManageViewModelAsync(
            Guid pageId,
            CancellationToken cancellationToken = default);

        Task<CmsManageSaveResult> SaveContentAsync(
            Guid pageId,
            IReadOnlyList<CmsContentValueUpdate> updates,
            string? userId,
            CancellationToken cancellationToken = default);
    }

    public sealed record CmsContentValueUpdate(Guid ContentId, string? ContentValue);

    public enum CmsManageSaveResult
    {
        Ok,
        PageNotFound,
        InvalidContent
    }
}
