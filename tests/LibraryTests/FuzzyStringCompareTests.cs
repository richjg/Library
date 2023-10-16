using System.Diagnostics;
using Library;
using NUnit.Framework;

namespace LibraryTests
{
    public class FuzzyStringCompareTests
    {
        [TestCase(null, null, 1)]
        [TestCase("", "", 1)]
        [TestCase("I'm Done", "I'm Done", 1)]
        [TestCase("I'm Done", "i'm done", 1)]
        [TestCase("I'm Done", "done", 0.5)]
        [TestCase("I'm Done", "i'm", 0.375)]
        [TestCase("I'm Done", "im done", 0.875)]
        [TestCase("Done", "don", 0.75)]
        [TestCase("Done", "doen", 0.75)]
        [TestCase("View tickets", "viwe ticket", 0.8299)]
        [TestCase("tickets", "ticket", 0.857)]
        [TestCase("查看门票", "查看门票", 1)]
        [TestCase("查看门票", "成为门票", 0.5)]
        [TestCase("honda", "ohnda", 0.80)]
        public void SimilarityDamareuLevenshtein_ReturnsExpectedValues(string src, string modified, double expected)
        {
            var fuzzyComparer = new FuzzyComparerDamareuLevenshtein();
            var result = fuzzyComparer.Similarity(src, modified);

            Assert.That(result, Is.EqualTo(expected).Within(.01));
        }

        [TestCase(null, null, 0)]
        [TestCase("", "", 0)]
        [TestCase("I'm Done", "I'm Done", 0)]
        [TestCase("I'm Done", "i'm done", 2)]
        [TestCase("I'm Done", "done", 5)]
        [TestCase("I'm Done", "i'm", 6)]
        [TestCase("I'm Done", "im done", 3)]
        [TestCase("Done", "don", 2)]
        [TestCase("Done", "doen", 2)]
        [TestCase("View tickets", "viwe ticket", 3)]
        [TestCase("tickets", "ticket", 1)]
        [TestCase("查看门票", "查看门票", 0)]
        [TestCase("查看门票", "成为门票", 2)]
        [TestCase("honda", "ohnda", 1)]
        //https://oldfashionedsoftware.com/tag/levenshtein-distance/
        [TestCase("example", "samples", 3)]
        [TestCase("sturgeon", "urgently", 6)]
        [TestCase("levenshtein", "frankenstein", 6)]
        [TestCase("distance", "difference", 5)]
        public void GetDistanceDamareuLevenshtein_ReturnsExpectedValues(string src, string modified, int expected)
        {
            var fuzzyComparer = new FuzzyComparerDamareuLevenshtein();
            var result = fuzzyComparer.GetDistance(src, modified);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test, Ignore("")]
        public void PerfTest()
        {
            var fuzzyComparers = new IFuzzyStringComparer[] { new FuzzyComparerDamareuLevenshtein() };

            for (int x = 0; x < 10; x++)
            {
                var perfResults = new Dictionary<string, List<double>>();

                foreach (var fuzzyComparer in fuzzyComparers)
                {
                    perfResults[fuzzyComparer.GetType().Name] = new List<double>();
                    var st = new Stopwatch();
                    for (int i = 0; i < 100000; i++)
                    {
                        st.Restart();
                        _ = fuzzyComparer.Similarity("View tickets", "viwe ticket");
                        st.Stop();

                        perfResults[fuzzyComparer.GetType().Name].Add(st.Elapsed.TotalMilliseconds);
                    }
                }

                foreach (var item in perfResults)
                {
                    Console.WriteLine($"{item.Key,-38} | {item.Value.Skip(1).Average(),-25} | {item.Value.Skip(1).Sum(),-25} | {item.Value.Skip(1).Min(),-25} | {item.Value.Skip(1).Max(),-25} | {CalculateStandardDeviation(item.Value.Skip(1)),-25}");
                }
            }

            static double CalculateStandardDeviation(IEnumerable<double> values)
            {
                int count = values.Count();

                double average = values.Average();
                double sum = values.Sum(d => Math.Pow(d - average, 2));
                double deviation = Math.Sqrt(sum / (count - 1));

                return deviation;
            }
        }
    }
}
