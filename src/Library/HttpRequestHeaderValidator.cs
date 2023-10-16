using System.Net.Http.Headers;

namespace Library
{
    public class HttpRequestHeaderValidator
    {
        private readonly HttpRequestHeaders requestHeaders;
        private readonly HttpContentHeaders contentHeaders;

        public HttpRequestHeaderValidator()
        {
            requestHeaders = new HttpRequestMessage().Headers;
            contentHeaders = new StringContent("").Headers;
            contentHeaders.Remove("Content-Type");
        }

        public bool TryAddIsValid(string key, string value) => TryAddIsValid(key, value, out _);
        public bool TryAddIsValid(string key, string value, out Exception? e)
        {
            e = null;
            try
            {
                if (key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    contentHeaders.Add(key, value);
                }
                else
                {
                    requestHeaders.Add(key, value);
                }
                return true;
            }
            catch (Exception ex)
            {
                e = ex;
                return false;
            }
        }
    }
}
