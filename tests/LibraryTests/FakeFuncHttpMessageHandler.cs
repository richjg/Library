using System.Net;
using System.Net.Http.Headers;

namespace LibraryTests
{
    public class FakeFuncHttpMessageHandler : HttpMessageHandler
    {
        public Uri? RequestUri { get; set; }
        public string? RequestContent { get; set; }
        public HttpMethod? RequestMethod { get; set; }
        public HttpRequestHeaders? RequestHeaders { get; set; }
        public MediaTypeHeaderValue? ContentType { get; set; }

        public Func<HttpRequestMessage, HttpResponseMessage>? HandleRequest { get; set; }
        public Func<HttpRequestMessage, Task<HttpResponseMessage>>? HandleRequestAsync { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestHeaders = request.Headers;
            RequestUri = request.RequestUri;
            RequestMethod = request.Method;
            ContentType = request.Content?.Headers?.ContentType;
            RequestContent = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            HttpResponseMessage? responseMessage = null;

            if (HandleRequest != null)
            {
                responseMessage = HandleRequest(request);
                return responseMessage ?? new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            if (HandleRequestAsync != null)
            {
                responseMessage = await HandleRequestAsync(request);
                return responseMessage ?? new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }

    public static class HttpRequestMessageExtensions
    {
        public static bool IsHttpPost(this HttpRequestMessage httpRequestMessage) => httpRequestMessage.Method == HttpMethod.Post;
        public static bool IsHttpPut(this HttpRequestMessage httpRequestMessage) => httpRequestMessage.Method == HttpMethod.Put;
        public static bool IsHttpGet(this HttpRequestMessage httpRequestMessage) => httpRequestMessage.Method == HttpMethod.Get;
        public static bool IsHttpPatch(this HttpRequestMessage httpRequestMessage) => httpRequestMessage.Method == HttpMethod.Patch;
        public static bool IsHttpDelete(this HttpRequestMessage httpRequestMessage) => httpRequestMessage.Method == HttpMethod.Delete;
        public static bool UrlEndsWith(this HttpRequestMessage httpRequestMessage, string value) => httpRequestMessage.RequestUri?.ToString()?.EndsWith(value) == true;
        public static bool UrlContains(this HttpRequestMessage httpRequestMessage, string value) => httpRequestMessage.RequestUri?.ToString()?.Contains(value) == true;
    }
    public static class HttpResponseMessageExtensions
    {
        public static HttpResponseMessage AddHeader(this HttpResponseMessage httpResponseMessage, string name, string value)
        {
            httpResponseMessage.Headers.Add(name, value);
            return httpResponseMessage;
        }

    }
}
