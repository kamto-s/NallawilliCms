namespace Nallawilli.Helpers.Public
{
    public static class CmsSectionViewLocations
    {
        public const string SectionsRoot = "~/Views/Page/Sections";

        /// <summary>
        /// Section code in admin must match partial filename exactly (e.g. _hero → _hero.cshtml).
        /// </summary>
        public static string GetPartialPath(string sectionCode) =>
            $"{SectionsRoot}/{sectionCode}.cshtml";
    }
}
