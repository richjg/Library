using Library;
using System.Text.RegularExpressions;

// ReSharper disable CheckNamespace
namespace Library
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string? value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsTrimmedNullOrEmpty(this string? value)
        {
            if (value == null)
                return true;

            return value.Trim().IsNullOrEmpty();
        }

        public static bool LengthIsGreaterThan(this string value, int maxLength) => value != null && value.Length > maxLength;

        public static bool IsValidUrl(this string value, UriKind uriKind = UriKind.Absolute)
        {
            if (value.IsTrimmedNullOrEmpty())
            {
                return false;
            }

            return Uri.TryCreate(value, uriKind, out _);
        }

        public static string TrimEnd(this string value, string toTrim)
        {
            return value.TrimEnd(toTrim, true);
        }

        public static string TrimEnd(this string value, string toTrim, bool ignoreCase)
        {
            StringComparison comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;

            if (value.EndsWith(toTrim, comparison))
            {
                return value.Remove(value.Length - toTrim.Length, toTrim.Length); ;
            }

            return value;
        }

        public static string Concat(this IEnumerable<string> strings, string delimiter)
        {
            if (strings == null || strings.Any() == false)
            {
                return string.Empty;
            }

            return string.Join(delimiter, strings.ToArray());
        }

        public static string ConcatDistinct(this IEnumerable<string> strings, string delimiter)
        {
            return strings.Distinct().Concat(delimiter);
        }

        public static string FormatValue(this string text, params object[] replacements)
        {
            return string.Format(text, replacements);
        }

        /// <summary>
        /// Checks if string is null before trim
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TrimSafe(this string value)
        {
            if (value.IsNullOrEmpty())
                return value;

            return value.Trim();
        }

        public static string Truncate(this string value, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "value has to be greater than or equal to zero");

            if (string.IsNullOrEmpty(value) || value.Length <= length)
                return value;

            return value.Substring(0, length);
        }

        public static string TruncateWithEllipsis(this string value, int length)
        {
            if (length < 3)
                throw new ArgumentOutOfRangeException(nameof(length), length, "value has to be greater than or equal to 3 as that is the length of '...'");

            if (string.IsNullOrEmpty(value) || value.Length <= length)
                return value;

            return value.Substring(0, length - 3) + "...";
        }

        /// <summary>
        /// If Prefix already added then won't be added again.
        /// </summary>
        /// <param name="value">The string you want to add the prefix to</param>
        /// <param name="prefixValue">
        /// prefixValue is case sensitive
        /// </param>
        public static string? PrefixWith(this string value, string prefixValue)
        {
            if (value == null)
                return value;

            return value.StartsWith(prefixValue) ? value : prefixValue + value;
        }

        public static bool Contains(this string value, string search, StringComparison stringComparison)
        {
            return value.IndexOf(search, stringComparison) >= 0;
        }

        public static int? ToInt(this string value)
        {
            if (int.TryParse(value, out var result))
            {
                return result;
            }

            return null;
        }

        public static int CountOcurrenencesOf(this string input, string pattern, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            if (input.IsTrimmedNullOrEmpty())
            {
                return 0;
            }

            if (pattern == null || pattern.Length == 0)
            {
                return 0;
            }

            int count = 0, index = 0, len = pattern.Length;

            while ((index = input.IndexOf(pattern, index, comparisonType)) != -1)
            {
                index += len;
                ++count;
            }

            return count;
        }

        public static string RegexReplace(this string input, string pattern, string replacement)
        {
            return Regex.Replace(input, pattern, replacement);
        }

        public static bool IsAllowedFileExtension(this string input, List<string> allowedFileExtensions)
            => Path.GetExtension(input).IsTrimmedNullOrEmpty() || allowedFileExtensions.Contains(Path.GetExtension(input), StringComparer.OrdinalIgnoreCase);

        public static IEnumerable<string> ToWords(this string input)
        {
            if (input.IsTrimmedNullOrEmpty())
                return Enumerable.Empty<string>();

            var punctuation = input.Where(char.IsPunctuation).Distinct().ToArray();
            return input.Split().Select(x => x.Trim(punctuation));
        }
    }
}
