using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LibraryTests
{
    //code take from net core source code we can test ReplaceAsync

    public static class RegexHelpers
    {
        public const string DefaultMatchTimeout_ConfigKeyName = "REGEX_DEFAULT_MATCH_TIMEOUT";

        public const int StressTestNestingDepth = 1000;

        /// <summary>RegexOptions.NonBacktracking.</summary>
        /// <remarks>Defined here to be able to reference the value by name even on .NET Framework test builds.</remarks>
        public const RegexOptions RegexOptionNonBacktracking = (RegexOptions)0x400;

        /// <summary>RegexOptions.NonBacktracking.</summary>
        /// <remarks>Defined here to be able to reference the value even in release builds.</remarks>
        public const RegexOptions RegexOptionDebug = (RegexOptions)0x80;

        public static bool IsDefaultCount(string input, RegexOptions options, int count)
        {
            if ((options & RegexOptions.RightToLeft) != 0)
            {
                return count == input.Length || count == -1;
            }
            return count == input.Length;
        }

        public static bool IsDefaultStart(string input, RegexOptions options, int start)
        {
            if ((options & RegexOptions.RightToLeft) != 0)
            {
                return start == input.Length;
            }
            return start == 0;
        }
        public static IEnumerable<object[]> AvailableEngines_MemberData =>
            from engine in AvailableEngines
            select new object[] { engine };

        public static IEnumerable<object[]> PrependEngines(IEnumerable<object[]> cases)
        {
            foreach (RegexEngine engine in AvailableEngines)
            {
                foreach (object[] additionalParameters in cases)
                {
                    var parameters = new object[additionalParameters.Length + 1];
                    additionalParameters.CopyTo(parameters, 1);
                    parameters[0] = engine;
                    yield return parameters;
                }
            }
        }

        public static IEnumerable<RegexEngine> AvailableEngines
        {
            get
            {
                yield return RegexEngine.Interpreter;
                yield return RegexEngine.Compiled;
            }
        }

        public static Task<Regex> GetRegexAsync(RegexEngine engine, string pattern, RegexOptions? options = null, TimeSpan? matchTimeout = null)
        {
            if (options is null)
            {
                Assert.Null(matchTimeout);
            }

            return Task.FromResult(
                options is null ? new Regex(pattern, OptionsFromEngine(engine)) :
                matchTimeout is null ? new Regex(pattern, options.Value | OptionsFromEngine(engine)) :
                new Regex(pattern, options.Value | OptionsFromEngine(engine), matchTimeout.Value));
        }

        public static Task<Regex[]> GetRegexesAsync(RegexEngine engine, params (string pattern, CultureInfo culture, RegexOptions? options, TimeSpan? matchTimeout)[] regexes)
        {
            var results = new Regex[regexes.Length];
            for (int i = 0; i < regexes.Length; i++)
            {
                (string pattern, CultureInfo culture, RegexOptions? options, TimeSpan? matchTimeout) = regexes[i];
                results[i] =
                    options is null ? new Regex(pattern, OptionsFromEngine(engine)) :
                    matchTimeout is null ? new Regex(pattern, options.Value | OptionsFromEngine(engine)) :
                    new Regex(pattern, options.Value | OptionsFromEngine(engine), matchTimeout.Value);
            }

            return Task.FromResult(results);
        }

        public static RegexOptions OptionsFromEngine(RegexEngine engine) => engine switch
        {
            RegexEngine.Interpreter => RegexOptions.None,
            RegexEngine.Compiled => RegexOptions.Compiled,
            RegexEngine.SourceGenerated => RegexOptions.Compiled,
            RegexEngine.NonBacktracking => RegexOptionNonBacktracking,
            RegexEngine.NonBacktrackingSourceGenerated => RegexOptionNonBacktracking | RegexOptions.Compiled,
            _ => throw new ArgumentException($"Unknown engine: {engine}"),
        };
    }

    public enum RegexEngine
    {
        Interpreter,
        Compiled,
        NonBacktracking,
        SourceGenerated,
        NonBacktrackingSourceGenerated,
    }

    public class CaptureData
    {
        private CaptureData(string value, int index, int length, bool createCaptures)
        {
            Value = value;
            Index = index;
            Length = length;

            // Prevent a StackOverflow recursion in the constructor
            if (createCaptures)
            {
                Captures = new CaptureData[] { new CaptureData(value, index, length, false) };
            }
        }

        public CaptureData(string value, int index, int length) : this(value, index, length, true)
        {
        }

        public CaptureData(string value, int index, int length, CaptureData[] captures) : this(value, index, length, false)
        {
            Captures = captures;
        }

        public string Value { get; } = string.Empty;
        public int Index { get; }
        public int Length { get; }
        public CaptureData[] Captures { get; } = Array.Empty<CaptureData>();
    }

}
