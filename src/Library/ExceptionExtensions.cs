using System.Collections;
using System.Text;

// ReSharper disable CheckNamespace
namespace Library
{
    public class JsonExceptionFormat
    {
        public string Type { get; set; } = "";
        public string Message { get; set; } = "";
        public string StackTrace { get; set; } = "";
        public Dictionary<string, object?> Data { get; set; } = new();
    }

    public static class ExceptionExtensions
    {
        public static T AddData<T>(this T exception, string key, string value) where T : Exception
        {
            exception.Data[key] = value;
            return exception;
        }

        public static T AddData<T>(this T exception, string key, object value) where T : Exception
        {
            exception.Data[key] = value;
            return exception;
        }

        public static Exception AddDataSafe(this Exception exception, string key, Func<string> getValue)
        {
            try
            {
                exception.AddData(key, getValue());
            }
            catch { /*Ignore errors*/ }

            return exception;
        }

        public static Exception AddDataSafe(this Exception exception, string key, Func<object> getValue)
        {
            try
            {
                exception.AddData(key, getValue());
            }
            catch { /*Ignore errors*/ }

            return exception;
        }

        public static async Task<Exception> AddDataSafeAsync(this Exception exception, string key, Func<Task<string>> getValue)
        {
            try
            {
                exception.AddData(key, await getValue());
            }
            catch { /*Ignore errors*/ }

            return exception;
        }

        public static async Task<Exception> AddDataSafeAsync(this Exception exception, string key, Func<Task<object>> getValue)
        {
            try
            {
                exception.AddData(key, await getValue());
            }
            catch { /*Ignore errors*/ }

            return exception;
        }

        public static async Task<Exception> AddDataSafeAsync(this Task<Exception> taskException, string key, Func<Task<string>> getValue)
        {
            var exception = await taskException;
            return await exception.AddDataSafeAsync(key, getValue);
        }

        public static Exception AddDataSafe<TData>(this Exception exception, string key, TData data, Func<TData, string> getValue)
        {
            try
            {
                exception.AddData(key, getValue(data));
            }
            catch { /*Ignore errors*/ }

            return exception;
        }

        public static Exception AddDataSafe<TData>(this Exception exception, TData data, Func<TData, IEnumerable<(string key, string value)>> getValues)
        {
            try
            {
                var kvps = getValues(data);

                foreach (var kvp in kvps)
                {
                    exception.AddData(kvp.key, kvp.value);
                }
            }
            catch { /*Ignore errors*/ }

            return exception;
        }

        public static string FormatAsString(this Exception exception)
        {
            var builder = new StringBuilder();

            foreach (var e in exception.GetAllExceptions())
            {
                builder.AppendLine(e.ToString());
                foreach (var item in e.GetAllExceptionDataItems())
                {
                    builder.AppendLine($"Exception Data Key:{item.Key} Value:{item.Value}");
                }
            }

            return builder.ToString();
        }

        public static string FormatAsJson(this Exception exception)
        {
            return exception.Format().ToJsonWithNoTypeNameHandling();
        }

        public static List<JsonExceptionFormat> Format(this Exception exception)
        {
            return exception.GetAllExceptions().Select(e =>
               new JsonExceptionFormat
               {
                   Type = e.GetType().Name,
                   Message = e.Message,
                   StackTrace = e.StackTrace ?? "",
                   Data = e.GetAllExceptionDataItems().Where(i => i.Key != null).ToDictionary(k => k.Key.ToString()!, k => k.Value)
               }).ToList();
        }

        public static IEnumerable<Exception> GetAllExceptions(this Exception ex)
        {
            var e = ex;
            do
            {
                yield return e;
                if (e is AggregateException agee)
                {
                    foreach (var innerAgee in agee.InnerExceptions.SelectMany(e1 => e1.GetAllExceptions()))
                    {
                        yield return innerAgee;
                    }
                    yield break;
                }
                e = e.InnerException;
            } while (e != null);
        }

        private static IEnumerable<DictionaryEntry> GetAllExceptionDataItems(this Exception ex)
        {
            foreach (var item in ex.Data.Cast<DictionaryEntry>().Where(de => de.Key is string).OrderBy(de => de.Key))
            {
                yield return item;
            }
        }
    }
}
