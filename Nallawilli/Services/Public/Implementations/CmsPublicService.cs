using Microsoft.EntityFrameworkCore;
using Nallawilli.Data;
using Nallawilli.Services.Public.Interfaces;
using Nallawilli.ViewModels;

namespace Nallawilli.Services.Public.Implementations
{
    public class CmsPublicService : ICmsPublicService
    {
        private readonly AppDbContext _db;

        public CmsPublicService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<CmsPublicPageViewModel?> GetPageBySlugAsync(
            string slug,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            var normalizedSlug = slug.Trim().ToLowerInvariant();

            var page = await _db.CmsPages
                .AsNoTracking()
                .Where(p => p.Slug == normalizedSlug && p.IsActive)
                .Select(p => new
                {
                    p.Slug,
                    p.Title,
                    p.MetaTitle,
                    p.MetaDescription,
                    Sections = p.Sections
                        .Where(s => s.IsActive)
                        .OrderBy(s => s.SortOrder)
                        .Select(s => new
                        {
                            s.SectionCode,
                            s.SectionName,
                            Contents = s.SectionContents
                                .OrderBy(c => c.SortOrder)
                                .Select(c => new
                                {
                                    c.ContentKey,
                                    c.ContentValue,
                                    c.InputType
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (page is null)
                return null;

            return new CmsPublicPageViewModel
            {
                Slug = page.Slug,
                Title = page.Title,
                PageTitle = page.MetaTitle ?? page.Title,
                MetaDescription = page.MetaDescription,
                Sections = page.Sections.Select(s =>
                {
                    var fields = s.Contents.ToDictionary(
                        c => c.ContentKey,
                        c => new CmsPublicFieldViewModel
                        {
                            ContentKey = c.ContentKey,
                            Value = c.ContentValue,
                            InputType = c.InputType
                        },
                        StringComparer.OrdinalIgnoreCase);

                    return new CmsPublicSectionViewModel
                    {
                        SectionCode = s.SectionCode,
                        SectionName = s.SectionName,
                        Fields = fields
                    };
                }).ToList()
            };
        }
    }
}
