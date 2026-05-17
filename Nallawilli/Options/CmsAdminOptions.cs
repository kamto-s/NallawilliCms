namespace Nallawilli.Options
{
    /// <summary>
    /// Admin CMS UI flags. When ShowPageStructureTools is false, hides master/structure UI
    /// (new page, page settings, delete, sections) and returns 404 for those actions.
    /// </summary>
    public class CmsAdminOptions
    {
        public const string SectionName = "CmsAdmin";

        public bool ShowPageStructureTools { get; set; } = true;
    }
}
