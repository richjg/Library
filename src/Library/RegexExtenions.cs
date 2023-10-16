// ReSharper disable CheckNamespace
using System.Text;
using System.Text.RegularExpressions;

namespace Library
{
    public static class RegexExtenions
    {
        public static async Task<string> ReplaceAsync(this Regex regex, string input, Func<Match, Task<string>> replacementFn)
        {
            //Simple implentation so the lambda can be async
            //If you have a regex that you think is correct but replace seems wrong it could be this code - run it using the non async version and see

            if (regex.Options.HasFlag(RegexOptions.RightToLeft))
            {
                throw new NotImplementedException("ReplaceAsync does no support RightToLeft");
            }

            var sb = new StringBuilder();
            var lastIndex = 0;

            foreach (Match match in regex.Matches(input))
            {
                sb.Append(input, lastIndex, match.Index - lastIndex)
                  .Append(await replacementFn(match).ConfigureAwait(false));

                lastIndex = match.Index + match.Length;
            }
            sb.Append(input, lastIndex, input.Length - lastIndex);
            return sb.ToString();
        }
    }

    public class Regexes
    {
        public static readonly Regex CssFontFamilyValidation = new Regex(@"^([a-zA-Z0-9-]+|(['""])[a-zA-Z0-9-\s]+\2)(\s*,\s*([a-zA-Z0-9-]+|(['""])[a-zA-Z0-9-\s]+\5))*$");
        public static readonly Regex CssLengthValidation = new Regex(@"^(0{1}$)|(\d+.?\d*(px|rem|em|ch|%|vh|vw|vmin|vmax))$");
        public class Css
        {
            /// <summary>
            /// A string or a comma seperated list of strings if the string has a space then must be enclosed in quotes e.g
            /// <para>Inherit</para> 
            /// <para>Arial, 'Times New Roman', "Franklin Gothic"</para> 
            /// </summary>
            public static bool IsFontFamilyValueValid(string value) => CssFontFamilyValidation.IsMatch(value);
            public static bool IsLengthValueValid(string value) => CssLengthValidation.IsMatch(value);
        }
    }
}
