using System.Net.Http.Headers;

namespace LibraryTests
{
    public class HttpClientHandlerFake : HttpMessageHandler
    {
        public HttpResponseMessage? HttpResponseMessage { get; set; }
        public Uri? RequestUri { get; set; }
        public string? RequestContent { get; set; }
        public HttpMethod? RequestMethod { get; set; }
        public HttpRequestHeaders? RequestHeaders { get; set; }
        public MediaTypeHeaderValue? ContentType { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestHeaders = request.Headers;
            RequestUri = request.RequestUri;
            RequestMethod = request.Method;
            ContentType = request.Content?.Headers?.ContentType;
            RequestContent = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            return Task.FromResult(HttpResponseMessage!);
        }
    }

    public class HttpClientHandlerFakeFunc : HttpMessageHandler
    {
        public Func<HttpResponseMessage>? HttpResponseMessageFunc { get; set; }

        public List<HttpClientHandlerFakeRequest> Requests { get; set; } = new List<HttpClientHandlerFakeRequest>();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new HttpClientHandlerFakeRequest
            {
                RequestHeaders = request.Headers,
                RequestUri = request.RequestUri,
                RequestMethod = request.Method,
                ContentType = request.Content?.Headers?.ContentType,
                RequestContent = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult()
            });

            return Task.FromResult(HttpResponseMessageFunc?.Invoke()!);
        }

        public class HttpClientHandlerFakeRequest
        {
            public Uri? RequestUri { get; set; }
            public string? RequestContent { get; set; }
            public HttpMethod? RequestMethod { get; set; }
            public HttpRequestHeaders? RequestHeaders { get; set; }
            public MediaTypeHeaderValue? ContentType { get; set; }
        }
    }
}