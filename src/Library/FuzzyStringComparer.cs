namespace Library
{
    public interface IFuzzyStringComparer
    {
        double Similarity(string src, string modified);
    }

    public class FuzzyComparerCaseInsenitiveExactMatch : IFuzzyStringComparer
    {
        public double Similarity(string src, string modified) => string.Equals(src, modified, StringComparison.OrdinalIgnoreCase) ? 1 : 0;
    }

    public class FuzzyComparerDamareuLevenshtein : IFuzzyStringComparer
    {
        public double Similarity(string src, string modified)
        {
            src ??= string.Empty;
            modified ??= string.Empty;

            if (string.Equals(src, modified, StringComparison.OrdinalIgnoreCase))
            {
                return 1d;
            }

            src = src.ToLowerInvariant();
            modified = modified.ToLowerInvariant();

            var distance = (double)Distance(src, modified);
            var result = 1d - distance / Math.Max(src.Length, modified.Length);
            return result;
        }

        public int GetDistance(string original, string modified) => Distance(original ?? string.Empty, modified ?? string.Empty);

        private int Distance(string original, string modified)
        {
            //this follows the https://en.wikipedia.org/wiki/Damerau%E2%80%93Levenshtein_distance
            //apprently lucence use Levenshtein https://lucene.apache.org/core/7_3_0/core/org/apache/lucene/search/FuzzyQuery.html
            //https://dzone.com/articles/the-levenshtein-algorithm-1

            int len_orig = original.Length;
            int len_diff = modified.Length;

            var matrix = new int[len_orig + 1, len_diff + 1];
            for (int i = 0; i <= len_orig; i++)
            {
                matrix[i, 0] = i;
            }

            for (int j = 0; j <= len_diff; j++)
            {
                matrix[0, j] = j;
            }

            var org = original.AsSpan();
            var mod = modified.AsSpan();
            for (int i = 1; i <= len_orig; i++)
            {
                for (int j = 1; j <= len_diff; j++)
                {
                    int cost = mod[j - 1] == org[i - 1] ? 0 : 1;

                    //below is 
                    //  minimum( d[i-1, j] + 1,              // deletion 
                    //           d[i, j - 1] + 1,            // insertion 
                    //           d[i - 1, j - 1] + cost)     // substitution
                    //  witten as
                    //  minimum( minimum (deletion, insertion) + 1, substitution)

                    matrix[i, j] = Math.Min(Math.Min(matrix[i - 1, j], matrix[i, j - 1]) + 1, matrix[i - 1, j - 1] + cost);

                    if (i > 1 && j > 1 && org[i - 1] == mod[j - 2] && org[i - 2] == mod[j - 1])
                    {
                        matrix[i, j] = Math.Min(matrix[i, j], matrix[i - 2, j - 2] + cost); //transposition making this Damareu
                    }
                }
            }

            ////debug show matrix
            //for (int i = 0; i < matrix.GetLength(0); i++)
            //{
            //    if (i == 0)
            //    {
            //        Console.Write("\t\t");
            //        foreach (var c in modified)
            //        {
            //            Console.Write(c + "\t");
            //        }
            //        Console.WriteLine();
            //    }

            //    for (int j = 0; j < matrix.GetLength(1); j++)
            //    {
            //        //(j == 0 && i > 0 && i < original.Length? original[i-1] + "\t" : " " + "\t" +

            //        if (i == 0 & j == 0)
            //        {
            //            Console.Write("\t");
            //        }

            //        if (i > 0 & j == 0)
            //        {
            //            Console.Write(original[i - 1] + "\t");
            //        }

            //        Console.Write(matrix[i, j] + "\t");
            //    }
            //    Console.WriteLine();
            //}

            //Console.WriteLine();
            //Console.WriteLine();
            //Console.WriteLine(matrix[len_orig, len_diff]);

            return matrix[len_orig, len_diff];
        }
    }
}
