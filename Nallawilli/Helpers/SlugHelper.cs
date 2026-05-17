using System.Text.RegularExpressions;

namespace Nallawilli.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string text)
        {
            text = text.ToLowerInvariant();

            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");

            text = Regex.Replace(text, @"\s+", "-").Trim('-');

            text = Regex.Replace(text, @"-+", "-");

            return text;
        }
    }
}
