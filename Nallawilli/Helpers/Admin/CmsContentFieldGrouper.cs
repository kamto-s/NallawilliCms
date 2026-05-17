using System.Text.RegularExpressions;

namespace Nallawilli.Helpers.Admin
{
    /// <summary>
    /// Groups manage fields by trailing index suffix (e.g. label_1, image_2 → item 1, item 2).
    /// </summary>
    public static partial class CmsContentFieldGrouper
    {
        public static CmsManageFieldsLayout BuildLayout(IReadOnlyList<string> contentKeys)
        {
            var ungrouped = new List<int>();
            var byIndex = new Dictionary<int, List<int>>();

            for (var i = 0; i < contentKeys.Count; i++)
            {
                var key = contentKeys[i];
                var match = TrailingIndex().Match(key);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var index))
                {
                    if (!byIndex.TryGetValue(index, out var list))
                    {
                        list = new List<int>();
                        byIndex[index] = list;
                    }

                    list.Add(i);
                }
                else
                {
                    ungrouped.Add(i);
                }
            }

            var groups = byIndex
                .OrderBy(kv => kv.Key)
                .Select(kv => new CmsManageIndexedGroup(kv.Key, kv.Value))
                .ToList();

            return new CmsManageFieldsLayout(ungrouped, groups);
        }

        [GeneratedRegex(@"_(\d+)$", RegexOptions.CultureInvariant)]
        private static partial Regex TrailingIndex();
    }

    public sealed class CmsManageFieldsLayout
    {
        public CmsManageFieldsLayout(
            IReadOnlyList<int> ungroupedFieldIndices,
            IReadOnlyList<CmsManageIndexedGroup> indexedGroups)
        {
            UngroupedFieldIndices = ungroupedFieldIndices;
            IndexedGroups = indexedGroups;
        }

        public IReadOnlyList<int> UngroupedFieldIndices { get; }

        public IReadOnlyList<CmsManageIndexedGroup> IndexedGroups { get; }

        public bool HasIndexedGroups => IndexedGroups.Count > 0;
    }

    public sealed class CmsManageIndexedGroup
    {
        public CmsManageIndexedGroup(int index, IReadOnlyList<int> fieldIndices)
        {
            Index = index;
            FieldIndices = fieldIndices;
        }

        public int Index { get; }

        public IReadOnlyList<int> FieldIndices { get; }

        public string Title => $"Item {Index}";
    }
}
