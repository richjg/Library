using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LibraryTests
{
    public class RegexExtensionsTests
    {
        //code take from net core source code we can test ReplaceAsync

        public static IEnumerable<object[]> Replace_MatchEvaluator_TestData()
        {
            foreach (RegexEngine engine in RegexHelpers.AvailableEngines)
            {
                yield return new object[] { engine, "a", "bbbb", new MatchEvaluator(match => "uhoh"), RegexOptions.None, "bbbb" };
                yield return new object[] { engine, "(Big|Small)", "Big mountain", new MatchEvaluator(MatchEvaluator1), RegexOptions.None, "Huge mountain" };
                yield return new object[] { engine, "(Big|Small)", "Small village", new MatchEvaluator(MatchEvaluator1), RegexOptions.None, "Tiny village" };

                if ("i".ToUpper() == "I")
                {
                    yield return new object[] { engine, "(Big|Small)", "bIG horse", new MatchEvaluator(MatchEvaluator1), RegexOptions.IgnoreCase, "Huge horse" };
                }

                yield return new object[] { engine, "(Big|Small)", "sMaLl dog", new MatchEvaluator(MatchEvaluator1), RegexOptions.IgnoreCase, "Tiny dog" };

                yield return new object[] { engine, ".+", "XSP_TEST_FAILURE", new MatchEvaluator(MatchEvaluator2), RegexOptions.None, "SUCCESS" };
                yield return new object[] { engine, "[abcabc]", "abcabc", new MatchEvaluator(MatchEvaluator3), RegexOptions.None, "ABCABC" };

                // Regression test:
                // Regex treating Devanagari matra characters as matching "\b"
                // Unicode characters in the "Mark, NonSpacing" Category, U+0902=Devanagari sign anusvara, U+0947=Devanagri vowel sign E
                string boldInput = "\u092f\u0939 \u0915\u0930 \u0935\u0939 \u0915\u0930\u0947\u0902 \u0939\u0948\u0964";
                string boldExpected = "\u092f\u0939 <b>\u0915\u0930</b> \u0935\u0939 <b>\u0915\u0930\u0947\u0902</b> \u0939\u0948\u0964";
                yield return new object[] { engine, @"\u0915\u0930.*?\b", boldInput, new MatchEvaluator(MatchEvaluatorBold), RegexOptions.CultureInvariant | RegexOptions.Singleline, boldExpected };
            }
        }

        [TestCaseSource(nameof(Replace_MatchEvaluator_TestData))]
        public async Task ReplaceAsync_MatchEvaluator_Test(RegexEngine engine, string pattern, string input, MatchEvaluator evaluator, RegexOptions options, string expected)
        {
            Regex r = await RegexHelpers.GetRegexAsync(engine, pattern, options);
            Assert.That(await r.ReplaceAsync(input, m => Task.FromResult(evaluator(m))), Is.EqualTo(expected));
        }
        public static IEnumerable<object[]> NoneCompiledBacktracking()
        {
            yield return new object[] { RegexOptions.None };
            yield return new object[] { RegexOptions.Compiled };
        }

        [TestCaseSource(nameof(NoneCompiledBacktracking))]
        public void Replace_NoMatch(RegexOptions options)
        {
            string input = "";
            Assert.That(Regex.Replace(input, "no-match", new MatchEvaluator(MatchEvaluator1), options), Is.EqualTo(input));
        }

        [TestCaseSource(nameof(NoneCompiledBacktracking))]
        public void Replace_MatchEvaluator_UniqueMatchObjects(RegexOptions options)
        {
            const string Input = "abcdefghijklmnopqrstuvwxyz";

            var matches = new List<Match>();

            string result = Regex.Replace(Input, @"[a-z]", match =>
            {
                Assert.That(match.Value, Is.EqualTo(((char)('a' + matches.Count)).ToString()));
                matches.Add(match);
                return match.Value.ToUpperInvariant();
            }, options);

            Assert.That(matches.Count, Is.EqualTo(26));
            Assert.That(result, Is.EqualTo("ABCDEFGHIJKLMNOPQRSTUVWXYZ"));

            Assert.That(string.Concat(matches.Cast<Match>().Select(m => m.Value)), Is.EqualTo(Input));
        }

        public static string MatchEvaluator1(Match match) => match.Value.ToLower() == "big" ? "Huge" : "Tiny";

        public static string MatchEvaluator2(Match match) => "SUCCESS";

        public static string MatchEvaluator3(Match match)
        {
            if (match.Value == "a" || match.Value == "b" || match.Value == "c")
                return match.Value.ToUpperInvariant();
            return string.Empty;
        }

        public static string MatchEvaluatorBold(Match match) => string.Format("<b>{0}</b>", match.Value);

        private static string MatchEvaluatorBar(Match match) => "bar";
        private static string MatchEvaluatorPoundSign(Match match) => "#";

        public static IEnumerable<object[]> TestReplaceWithToUpperMatchEvaluator_TestData()
        {
            foreach (object[] data in NoneCompiledBacktracking())
            {
                RegexOptions options = (RegexOptions)data[0];
                yield return new object[] { @"(\bis\b)", "this is it", "this IS it", options };
            }
        }

        [TestCaseSource(nameof(TestReplaceWithToUpperMatchEvaluator_TestData))]
        public async Task TestReplaceWithToUpperMatchEvaluator(string pattern, string input, string expectedoutput, RegexOptions opt)
        {
            MatchEvaluator f = new MatchEvaluator(m => m.Value.ToUpper());
            var output = await new Regex(pattern, opt).ReplaceAsync(input, m => Task.FromResult(f(m)));

            Assert.That(output, Is.EqualTo(expectedoutput));
        }

        //Our Tests

        [Test]
        public async Task ReplaceAsync_ReplacesAsExpected()
        {
            var input = "Start ##tenjin:b64download:http://www.biomni.com## middle ##tenjin:b64download:http://www.biomni.com## end";

            var result = await new Regex(@"##tenjin:b64download:(?<url>.*?)##").ReplaceAsync(input, async m =>
            {
                await Task.Yield();

                var url = m.Groups["url"]?.Value;
                if (url.IsTrimmedNullOrEmpty())
                {
                    return m.Value;
                }

                return "123";
            });

            Assert.That(result, Is.EqualTo("Start 123 middle 123 end"));
        }

        [Test]
        public void ReplaceAsync_ThrowsWhenRightToLeft()
        {
            var input = "";

            async Task<string> Act()
            {
                return await new Regex(@"a", RegexOptions.RightToLeft).ReplaceAsync(input, m =>
                {
                    return m.Value.AsCompletedTask();
                });
            }

            var exception = Assert.ThrowsAsync<NotImplementedException>(Act);
        }
    }
}
