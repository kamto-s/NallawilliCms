namespace Nallawilli.Helpers.Common
{
    public static class CmsInputTypes
    {
        public const string Text = "text";
        public const string TextArea = "textarea";
        public const string Image = "image";
        public const string Url = "url";
        public const string RichText = "richtext";
        public const string Number = "number";
        public const string Boolean = "bool";
        public const string Color = "color";

        public static IReadOnlyList<string> All { get; } = new[]
        {
            Text,
            TextArea,
            Image,
            Url,
            RichText,
            Number,
            Boolean,
            Color
        };

        public static bool IsValid(string? inputType) =>
            !string.IsNullOrWhiteSpace(inputType) &&
            All.Contains(inputType.Trim(), StringComparer.OrdinalIgnoreCase);
    }
}
