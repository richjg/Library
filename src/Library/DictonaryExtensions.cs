// ReSharper disable CheckNamespace

using Library;

namespace Library
{
    public static class DictonaryExtensions
    {
        public static bool TryGetValueSafe<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue? value)
        {
            value = default;
            return dictionary?.TryGetValue(key, out value) ?? false;
        }

        public static TValue? TryGetValueSafe<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? valueIfMissing = default)
        {
            var value = default(TValue);
            return dictionary?.TryGetValue(key, out value) == true ? value : valueIfMissing;
        }

        public static bool TryGetValueAsLong<TKey>(this IDictionary<TKey, string> dictionary, TKey key, out long value)
        {
            value = 0;

            if (dictionary != null)
            {
                if (dictionary.TryGetValue(key, out var stringValue))
                {
                    if (stringValue.IsTrimmedNullOrEmpty() == false)
                    {
                        if (long.TryParse(stringValue, out value))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static Dictionary<string, object> Merge(this Dictionary<string, object> dic1, Dictionary<string, object> dic2)
        {
            if (dic1 == null && dic2 == null)
            {
                return new Dictionary<string, object>();
            }

            if (dic1 != null && dic2 == null)
            {
                return new Dictionary<string, object>(dic1);
            }

            if (dic1 == null && dic2 != null)
            {
                return new Dictionary<string, object>(dic2);
            }

            return dic1!.Concat(dic2!).GroupBy(k => k.Key).ToDictionary(g => g.Key, g =>
            {
                var values = g.Select(k => k.Value).ToList();
                if (values.Count == 1 || values.Distinct().Count() == 1)
                {
                    return values[0];
                }

                return values;
            });
        }
        public static List<KeyValuePair<string, string>> AddOrReplace(this List<KeyValuePair<string, string>> keyValuePairs, string key, string value)
        {
            var currentIndex = keyValuePairs.IndexOf(keyValuePairs.FirstOrDefault(kv => kv.Key.Equals(key, StringComparison.OrdinalIgnoreCase)));
            if (currentIndex > -1)
                keyValuePairs.RemoveAt(currentIndex);

            keyValuePairs.Add(new KeyValuePair<string, string>(key, value));

            return keyValuePairs;
        }
    }
}
