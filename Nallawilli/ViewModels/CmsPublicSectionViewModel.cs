using Nallawilli.Helpers;

namespace Nallawilli.ViewModels
{
    public sealed class CmsPublicSectionViewModel
    {
        public string SectionCode { get; init; } = string.Empty;
         
        public string SectionName { get; init; } = string.Empty;

        public IReadOnlyDictionary<string, CmsPublicFieldViewModel> Fields { get; init; } =
            new Dictionary<string, CmsPublicFieldViewModel>(StringComparer.OrdinalIgnoreCase);

        public string? Get(string contentKey) =>
            Fields.TryGetValue(contentKey, out var field) ? field.Value : null;

        public bool IsRichText(string contentKey) =>
            string.Equals(
                Fields.TryGetValue(contentKey, out var field) ? field.InputType : null,
                CmsInputTypes.RichText,
                StringComparison.OrdinalIgnoreCase);
    }

    public sealed class CmsPublicFieldViewModel
    {
        public string ContentKey { get; init; } = string.Empty;

        public string? Value { get; init; }

        public string InputType { get; init; } = CmsInputTypes.Text;
    }
}
