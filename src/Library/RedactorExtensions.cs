using System.Web;

// ReSharper disable CheckNamespace
namespace Library
{
    public static class RedactorExtensions
    {
        public static string RedactJson(this string json) => Redactor.Instance.Redact(json);

        public static string RedactHeaders(this IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers) => Redactor.Instance.Redact(headers);

        public static string RedactUrlEncodedContent(this string urlEncodedContent)
        {
            var nameValueCollection = HttpUtility.ParseQueryString(urlEncodedContent);
            return nameValueCollection.AllKeys.ToDictionary(t => t!, t => nameValueCollection[t]).ToJsonWithNoTypeNameHandling().RedactJson();
        }
    }
}