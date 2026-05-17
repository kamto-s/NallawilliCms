using System.Globalization;
using System.Text.RegularExpressions;

namespace Nallawilli.Helpers.Admin
{
    public static partial class CmsColorHelper
    {
        public const string DefaultColor = "#55b7a8";

        public static string ToPickerHex(string? value)
        {
            if (TryToHex(value, out var hex))
                return hex;

            return DefaultColor;
        }

        public static string ResolveValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DefaultColor;

            var trimmed = value.Trim();
            return TryToHex(trimmed, out var hex) ? hex : trimmed;
        }

        public static bool TryToHex(string? value, out string hex)
        {
            hex = DefaultColor;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var v = value.Trim();

            var hex6 = Hex6OptionalHash().Match(v);
            if (hex6.Success)
            {
                hex = "#" + hex6.Groups[1].Value.ToLowerInvariant();
                return true;
            }

            var hex3 = Hex3OptionalHash().Match(v);
            if (hex3.Success)
            {
                var h = hex3.Groups[1].Value;
                hex = $"#{h[0]}{h[0]}{h[1]}{h[1]}{h[2]}{h[2]}".ToLowerInvariant();
                return true;
            }

            var rgb = RgbFunction().Match(v);
            if (rgb.Success
                && TryRgbComponent(rgb.Groups[1].Value, out var r)
                && TryRgbComponent(rgb.Groups[2].Value, out var g)
                && TryRgbComponent(rgb.Groups[3].Value, out var b))
            {
                hex = $"#{r:x2}{g:x2}{b:x2}";
                return true;
            }

            return false;
        }

        private static bool TryRgbComponent(string raw, out int component)
        {
            component = 0;
            if (!int.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                return false;

            component = Math.Clamp(n, 0, 255);
            return true;
        }

        [GeneratedRegex(@"^#?([0-9a-fA-F]{6})$")]
        private static partial Regex Hex6OptionalHash();

        [GeneratedRegex(@"^#?([0-9a-fA-F]{3})$")]
        private static partial Regex Hex3OptionalHash();

        [GeneratedRegex(
            @"^rgba?\(\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})",
            RegexOptions.IgnoreCase)]
        private static partial Regex RgbFunction();
    }
}
