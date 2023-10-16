using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly.Timeout;

// ReSharper disable CheckNamespace
namespace Library
{
    public static class HttpResponseMessageExtensions
    {
        const int JSONREADER_MAXDEPTH = 1000;

        public static IHttpClientFluent NoLogin(this HttpClient httpClient)
        {
            return new HttpClientFluent(httpClient);
        }

        public static IHttpClientFluent WithGetPolicy(this IHttpClientFluent httpClientFluent)
        {
            ((HttpClientFluent)httpClientFluent).Headers.Add(new KeyValuePair<string, string>("http-policy", "get"));
            return httpClientFluent;
        }

        public static IHttpClientFluent WithGetPolicy(this HttpClient httpClient)
        {
            return httpClient.WithHeader("http-policy", "get");
        }

        public static IHttpClientFluent WithSubscriptionKey(this HttpClient httpClient, string subscriptionKey)
        {
            return httpClient.WithHeader("Ocp-Apim-Subscription-Key", subscriptionKey);
        }

        public static IHttpClientFluent WithEndpointKey(this HttpClient httpClient, string endpointKey)
        {
            return httpClient.WithHeader("Authorization", $"EndpointKey {endpointKey}");
        }

        public static IHttpClientFluent WithBearer(this HttpClient httpClient, string token)
        {
            return httpClient.WithHeader("Authorization", $"Bearer {token}");
        }

        public static IHttpClientFluent WithApiKey(this HttpClient httpClient, string apiKey)
        {
            return httpClient.WithHeader("api-key", apiKey);
        }

        public static IHttpClientFluent WithBasicAuthentication(this HttpClient httpClient, string username, string password)
        {
            var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.Default.GetBytes($"{username}:{password}")));
            return httpClient.WithHeader("Authorization", authHeader.ToString());
        }

        public static IHttpClientFluent WithAzureStorageServiceConnection(this HttpClient httpClient, string token)
        {
            return httpClient.WithHeaders(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("x-ms-version", "2017-11-09"),
                new KeyValuePair<string, string>("x-ms-blob-type", "BlockBlob"),
                new KeyValuePair<string, string>("Authorization", $"Bearer {token}")
            });
        }

        public static IHttpClientFluent AddHeader(this IHttpClientFluent httpClientFluent, string header, string value)
        {
            ((HttpClientFluent)httpClientFluent).Headers.Add(new KeyValuePair<string, string>(header, value));
            return httpClientFluent;
        }

        public static IHttpClientFluent IfValueAddHeader(this IHttpClientFluent httpClientFluent, string header, string value)
        {
            if (value.IsTrimmedNullOrEmpty() == false)
                ((HttpClientFluent)httpClientFluent).Headers.Add(new KeyValuePair<string, string>(header, value));

            return httpClientFluent;
        }

        public static IHttpClientFluent IfNotNullAddHeader(this IHttpClientFluent httpClientFluent, string header, string value)
        {
            if (value != null)
                ((HttpClientFluent)httpClientFluent).Headers.Add(new KeyValuePair<string, string>(header, value));

            return httpClientFluent;
        }

        public static IHttpClientFluent WithLanguage(this IHttpClientFluent httpClientFluent, string cultureCode)
        {
            ((HttpClientFluent)httpClientFluent).Headers.Add(new KeyValuePair<string, string>("Accept-Language", cultureCode));
            return httpClientFluent;
        }

        public static IHttpClientFluent WithHeader(this HttpClient httpClient, string httpRequestHeader, string headerValue)
        {
            return httpClient.WithHeaders(new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(httpRequestHeader, headerValue) });
        }

        public static IHttpClientFluent WithHeaders(this HttpClient httpClient, List<KeyValuePair<string, string>> headers)
        {
            return new HttpClientFluent(httpClient) { Headers = headers };
        }

        public static IHttpClientFluent WithoutHeader(this HttpClient httpClient)
        {
            return new HttpClientFluent(httpClient);
        }

        public static IHttpClientFluent WithHeadersIfAny(this HttpClient httpClient, List<KeyValuePair<string, string>> headers)
        {
            if (headers.Any())
                return new HttpClientFluent(httpClient) { Headers = headers };

            return new HttpClientFluent(httpClient);
        }

        public static Task<HttpResponseMessage> GetAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).GetAsync(requestUri);
        }
        public static Task<HttpResponseMessage> GetAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, string content, string contentType)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).GetAsync(requestUri, content, contentType);
        }
        public static Task<HttpResponseMessage> GetFormAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, Dictionary<string, string> formDictionary, string contentType)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).GetFormAsync(requestUri, formDictionary, contentType);
        }
        public static Task<HttpResponseMessage> PostStringAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, string content)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PostStringAsync(requestUri, content);
        }
        public static Task<HttpResponseMessage> PostStringAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, string content, string contentType)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PostStringAsync(requestUri, content, contentType);
        }
        public static Task<HttpResponseMessage> PostJsonAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, object jsonObject)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PostJsonAsync(requestUri, jsonObject);
        }
        public static Task<HttpResponseMessage> PostJsonAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, object jsonObject, HttpCompletionOption httpCompletionOption)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PostJsonAsync(requestUri, jsonObject, httpCompletionOption);
        }
        public static Task<HttpResponseMessage> PostJsonAsync(this IHttpClientFluent httpRequestMessageFluent, Uri requestUri, object jsonObject)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PostJsonAsync(requestUri, jsonObject);
        }
        public static Task<HttpResponseMessage> PostAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PostAsync(requestUri);
        }
        public static Task<HttpResponseMessage> PostAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, HttpContent content)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PostAsync(requestUri, content);
        }
        public static Task<HttpResponseMessage> PostFormAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, Dictionary<string, string> formDictionary)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PostFormAsync(requestUri, formDictionary);
        }
        public static Task<HttpResponseMessage> PostFormAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, Dictionary<string, string> formDictionary, string contentType)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PostFormAsync(requestUri, formDictionary, contentType);
        }
        public static Task<HttpResponseMessage> PostFormAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, MultipartFormDataContent multipartFormDataContent)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PostFormAsync(requestUri, multipartFormDataContent);
        }
        public static Task<HttpResponseMessage> PostBytesAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, byte[] bytes)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PostBytesAsync(requestUri, bytes);
        }
        public static Task<HttpResponseMessage> PostBytesAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, byte[] bytes, string contentType)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PostBytesAsync(requestUri, bytes, contentType);
        }
        public static Task<HttpResponseMessage> PutAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PutAsync(requestUri);
        }
        public static Task<HttpResponseMessage> PutJsonAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, object jsonObject)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PutJsonAsync(requestUri, jsonObject);
        }
        public static Task<HttpResponseMessage> PutStringAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, string content, string contentType)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PutStringAsync(requestUri, content, contentType);
        }
        public static Task<HttpResponseMessage> PutBytesAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, byte[] bytes)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PutBytesAsync(requestUri, bytes);
        }
        public static Task<HttpResponseMessage> PutBytesAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, byte[] bytes, string contentType)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PutBytesAsync(requestUri, bytes, contentType);
        }
        public static Task<HttpResponseMessage> PutFormAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, Dictionary<string, string> formDictionary, string contentType)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PutFormAsync(requestUri, formDictionary, contentType);
        }
        public static Task<HttpResponseMessage> PatchAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PatchAsync(requestUri);
        }
        public static Task<HttpResponseMessage> PatchStringAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, string content, string contentType)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PatchStringAsync(requestUri, content, contentType);
        }
        public static Task<HttpResponseMessage> PatchJsonAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, object jsonObject)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PatchJsonAsync(requestUri, jsonObject);
        }
        public static Task<HttpResponseMessage> PatchFormAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, Dictionary<string, string> formDictionary, string contentType)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PatchFormAsync(requestUri, formDictionary, contentType);
        }
        public static Task<HttpResponseMessage> PatchBytesAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, byte[] bytes)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PatchBytesAsync(requestUri, bytes);
        }
        public static Task<HttpResponseMessage> PatchBytesAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, byte[] bytes, string contentType)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).PatchBytesAsync(requestUri, bytes, contentType);
        }
        public static Task<HttpResponseMessage> DeleteAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).DeleteAsync(requestUri);
        }
        public static Task<HttpResponseMessage> DeleteAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, string content, string contentType)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).DeleteAsync(requestUri, content, contentType);
        }
        public static Task<HttpResponseMessage> DeleteFormAsync(this IHttpClientFluent httpRequestMessageFluent, string requestUri, Dictionary<string, string> formDictionary, string contentType)
        {
            return ((HttpClientFluent)httpRequestMessageFluent).DeleteFormAsync(requestUri, formDictionary, contentType);
        }
        public static Task<IHttpResponseMessageFluentStart<T?>> IfSuccessAsync<T>(this Task<HttpResponseMessage> httpResponseMessageTask, Func<HttpResponseMessage, Task<T>> handler)
        {
            IHttpResponseMessageFluentStart<T?> httpResponseMessageFluent = new HttpResponseMessageFluent<T>(httpResponseMessageTask).AddReponseMessageHandler((r) => r.IsSuccessStatusCode, async (r) =>
            {
                return await handler(r);
            });

            return Task.FromResult(httpResponseMessageFluent);
        }
        public static Task<IHttpResponseMessageFluentStart<T?>> IfStatusCodeAsync<T>(this Task<HttpResponseMessage> httpResponseMessageTask, HttpStatusCode statusCode, Func<HttpResponseMessage, Task<T>> handler)
        {
            IHttpResponseMessageFluentStart<T?> httpResponseMessageFluent = new HttpResponseMessageFluent<T>(httpResponseMessageTask).AddReponseMessageHandler((r) => r.StatusCode == statusCode, async (r) =>
            {
                return await handler(r);
            });

            return Task.FromResult(httpResponseMessageFluent);
        }

        public static async Task<IHttpResponseMessageFluentStart<T>> ElseIfTimeoutAsync<T>(this Task<IHttpResponseMessageFluentStart<T>> httpResponseMessageFluentStartTask, Func<Task<T>> handler)
        {
            var httpResponseMessageFluentStart = await httpResponseMessageFluentStartTask;

            return ((HttpResponseMessageFluent<T>)httpResponseMessageFluentStart).AddExceptionHandler((e) => e is TaskCanceledException || e is TimeoutRejectedException, async (e) =>
            {
                return await handler();
            });
        }

        public static async Task<IHttpResponseMessageFluentMiddle<T>> ElseIfTimeoutAsync<T>(this Task<IHttpResponseMessageFluentMiddle<T>> httpResponseMessageFluentMiddleTask, Func<Task<T>> handler)
        {
            var httpResponseMessageFluentMiddle = await httpResponseMessageFluentMiddleTask;

            return ((HttpResponseMessageFluent<T>)httpResponseMessageFluentMiddle).AddExceptionHandler((e) => e is TaskCanceledException || e is TimeoutRejectedException, async (e) =>
            {
                return await handler();
            });
        }

        public static async Task<IHttpResponseMessageFluentStart<T>> ElseIfHttpRequestExceptionAsync<T>(this Task<IHttpResponseMessageFluentStart<T>> httpResponseMessageFluentStartTask, Func<HttpRequestException, Task<T>> handler)
        {
            var httpResponseMessageFluentStart = await httpResponseMessageFluentStartTask;

            return ((HttpResponseMessageFluent<T>)httpResponseMessageFluentStart).AddExceptionHandler((e) => e is HttpRequestException, async (e) =>
            {
                return await handler((e as HttpRequestException)!);
            });
        }

        public static async Task<IHttpResponseMessageFluentMiddle<T>> ElseIfHttpRequestExceptionAsync<T>(this Task<IHttpResponseMessageFluentMiddle<T>> httpResponseMessageFluentMiddleTask, Func<HttpRequestException, Task<T>> handler)
        {
            var httpResponseMessageFluentMiddle = await httpResponseMessageFluentMiddleTask;

            return ((HttpResponseMessageFluent<T>)httpResponseMessageFluentMiddle).AddExceptionHandler((e) => e is HttpRequestException, async (e) =>
            {
                return await handler((e as HttpRequestException)!);
            });
        }


        public static Task<IHttpResponseMessageFluentMiddle?> ElseIfNotFoundAsync(this Task<IHttpResponseMessageFluentStart> httpResponseMessageFluentStartTask, Func<HttpResponseMessage, Task> handler) => httpResponseMessageFluentStartTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.NotFound, handler);
        public static Task<IHttpResponseMessageFluentMiddle?> ElseIfNotFoundAsync(this Task<IHttpResponseMessageFluentMiddle> httpResponseMessageFluentMiddleTask, Func<HttpResponseMessage, Task> handler) => httpResponseMessageFluentMiddleTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.NotFound, handler);
        public static Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfNotFoundAsync<T>(this Task<IHttpResponseMessageFluentStart<T>> httpResponseMessageFluentStartTask, Func<HttpResponseMessage, Task<T>> handler) => httpResponseMessageFluentStartTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.NotFound, handler);
        public static Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfNotFoundAsync<T>(this Task<IHttpResponseMessageFluentMiddle<T>> httpResponseMessageFluentMiddleTask, Func<HttpResponseMessage, Task<T>> handler) => httpResponseMessageFluentMiddleTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.NotFound, handler);

        public static Task<IHttpResponseMessageFluentMiddle?> ElseIfConflictAsync(this Task<IHttpResponseMessageFluentStart> httpResponseMessageFluentStartTask, Func<HttpResponseMessage, Task> handler) => httpResponseMessageFluentStartTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.Conflict, handler);
        public static Task<IHttpResponseMessageFluentMiddle?> ElseIfConflictAsync(this Task<IHttpResponseMessageFluentMiddle> httpResponseMessageFluentMiddleTask, Func<HttpResponseMessage, Task> handler) => httpResponseMessageFluentMiddleTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.Conflict, handler);
        public static Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfConflictAsync<T>(this Task<IHttpResponseMessageFluentStart<T>> httpResponseMessageFluentStartTask, Func<HttpResponseMessage, Task<T>> handler) => httpResponseMessageFluentStartTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.Conflict, handler);
        public static Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfConflictAsync<T>(this Task<IHttpResponseMessageFluentMiddle<T>> httpResponseMessageFluentMiddleTask, Func<HttpResponseMessage, Task<T>> handler) => httpResponseMessageFluentMiddleTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.Conflict, handler);

        public static Task<IHttpResponseMessageFluentMiddle?> ElseIfTooManyRequestsAsync(this Task<IHttpResponseMessageFluentStart> httpResponseMessageFluentStartTask, Func<HttpResponseMessage, Task> handler) => httpResponseMessageFluentStartTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.TooManyRequests, handler);
        public static Task<IHttpResponseMessageFluentMiddle?> ElseIfTooManyRequestsAsync(this Task<IHttpResponseMessageFluentMiddle> httpResponseMessageFluentMiddleTask, Func<HttpResponseMessage, Task> handler) => httpResponseMessageFluentMiddleTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.TooManyRequests, handler);
        public static Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfTooManyRequestsAsync<T>(this Task<IHttpResponseMessageFluentStart<T>> httpResponseMessageFluentStartTask, Func<HttpResponseMessage, Task<T>> handler) => httpResponseMessageFluentStartTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.TooManyRequests, handler);
        public static Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfTooManyRequestsAsync<T>(this Task<IHttpResponseMessageFluentMiddle<T>> httpResponseMessageFluentMiddleTask, Func<HttpResponseMessage, Task<T>> handler) => httpResponseMessageFluentMiddleTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.TooManyRequests, handler);

        public static Task<IHttpResponseMessageFluentMiddle?> ElseIfBadRequestAsync(this Task<IHttpResponseMessageFluentStart> httpResponseMessageFluentStartTask, Func<HttpResponseMessage, Task> handler) => httpResponseMessageFluentStartTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.BadRequest, handler);
        public static Task<IHttpResponseMessageFluentMiddle?> ElseIfBadRequestAsync(this Task<IHttpResponseMessageFluentMiddle> httpResponseMessageFluentMiddleTask, Func<HttpResponseMessage, Task> handler) => httpResponseMessageFluentMiddleTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.BadRequest, handler);
        public static Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfBadRequestAsync<T>(this Task<IHttpResponseMessageFluentStart<T>> httpResponseMessageFluentStartTask, Func<HttpResponseMessage, Task<T>> handler) => httpResponseMessageFluentStartTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.BadRequest, handler);
        public static Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfBadRequestAsync<T>(this Task<IHttpResponseMessageFluentMiddle<T>> httpResponseMessageFluentMiddleTask, Func<HttpResponseMessage, Task<T>> handler) => httpResponseMessageFluentMiddleTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.BadRequest, handler);

        public static Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfUnauthorizedAsync<T>(this Task<IHttpResponseMessageFluentStart<T>> httpResponseMessageFluentStartTask, Func<HttpResponseMessage, Task<T>> handler) => httpResponseMessageFluentStartTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.Unauthorized, handler);
        public static Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfUnauthorizedAsync<T>(this Task<IHttpResponseMessageFluentMiddle<T>> httpResponseMessageFluentMiddleTask, Func<HttpResponseMessage, Task<T>> handler) => httpResponseMessageFluentMiddleTask.ElseIfHttpStatusCodeAsync(HttpStatusCode.Unauthorized, handler);

        public static Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfIn500RangeRequestAsync<T>(this Task<IHttpResponseMessageFluentStart<T>> httpResponseMessageFluentStartTask, Func<HttpResponseMessage, Task<T>> handler) => httpResponseMessageFluentStartTask.ElseIfPredicateAsync(IsInStatus500Range, handler);
        public static Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfIn500RangeRequestAsync<T>(this Task<IHttpResponseMessageFluentMiddle<T>> httpResponseMessageFluentMiddleTask, Func<HttpResponseMessage, Task<T>> handler) => httpResponseMessageFluentMiddleTask.ElseIfPredciateAsync(IsInStatus500Range, handler);


        public static Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfStatusCodeAsync<T>(this Task<IHttpResponseMessageFluentStart<T>> httpResponseMessageFluentMiddleTask, HttpStatusCode statusCode, Func<HttpResponseMessage, Task<T>> handler) => httpResponseMessageFluentMiddleTask.ElseIfHttpStatusCodeAsync(statusCode, handler);
        public static Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfStatusCodeAsync<T>(this Task<IHttpResponseMessageFluentMiddle<T>> httpResponseMessageFluentMiddleTask, HttpStatusCode statusCode, Func<HttpResponseMessage, Task<T>> handler) => httpResponseMessageFluentMiddleTask.ElseIfHttpStatusCodeAsync(statusCode, handler);

        public static async Task<T?> ElseThrowAsync<T>(this Task<IHttpResponseMessageFluentStart<T>> httpResponseMessageFluentStartTask)
        {
            var httpResponseMessageFluentStart = await httpResponseMessageFluentStartTask;
            return await ((HttpResponseMessageFluent<T>)httpResponseMessageFluentStart).AddReponseMessageHandler(r => r.IsSuccessStatusCode == false, (r) => ElseThrowImplAsync<T>(r)).EvaluateResponseAsync();
        }
        public static async Task<T?> ElseThrowAsync<T>(this Task<IHttpResponseMessageFluentMiddle<T>> httpResponseMessageFluentMiddleTask)
        {
            var httpResponseMessageFluentMiddle = await httpResponseMessageFluentMiddleTask;
            return await ((HttpResponseMessageFluent<T>)httpResponseMessageFluentMiddle).AddReponseMessageHandler(r => r.IsSuccessStatusCode == false, (r) => ElseThrowImplAsync<T>(r)).EvaluateResponseAsync();
        }
        public static async Task<T?> ContentFromJsonAsync<T>(this HttpResponseMessage httpResponseMessage)
        {
            var content = await httpResponseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings { MaxDepth = JSONREADER_MAXDEPTH });
        }
        public static Task<T?> ContentFromJsonAsync<T>(this HttpResponseMessage httpResponseMessage, T anonymous) => httpResponseMessage.ContentFromJsonAsync<T>();
        public static async Task<T?> ContentFromXmlAsync<T>(this HttpResponseMessage httpResponseMessage)
        {
            var content = await httpResponseMessage.Content.ReadAsStringAsync();

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(content))
            {
                return (T?)serializer.Deserialize(reader);
            }
        }
        public static async Task<T?> ContentFromJsonIgnoringErrorsAsync<T>(this HttpResponseMessage httpResponseMessage)
        {
            var content = await httpResponseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings { MaxDepth = JSONREADER_MAXDEPTH, Error = (sender, errorArgs) => { errorArgs.ErrorContext.Handled = true; } });
        }
        public static async Task<Stream> ContentAsStream(this HttpResponseMessage httpResponseMessage)
        {
            return await httpResponseMessage.Content.ReadAsStreamAsync();
        }

        public static HttpRequestMessage WithHeaders(this HttpRequestMessage httpRequestMessage, IEnumerable<KeyValuePair<string, string>> headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    httpRequestMessage.Headers.Add(header.Key, header.Value);
                }
            }
            return httpRequestMessage;
        }
        public static HttpRequestMessage WithJsonContent(this HttpRequestMessage httpRequestMessage, object jsonObject)
        {
            httpRequestMessage.Content = new StringContent(SerializeJsonObject(jsonObject), Encoding.UTF8);
            httpRequestMessage.Content.Headers.Remove("Content-Type");
            httpRequestMessage.Content.Headers.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

            return httpRequestMessage;
        }
        public static HttpRequestMessage WithStringContent(this HttpRequestMessage httpRequestMessage, string content)
        {
            httpRequestMessage.Content = new StringContent(content, Encoding.UTF8);
            httpRequestMessage.Content.Headers.Remove("Content-Type");
            httpRequestMessage.Content.Headers.TryAddWithoutValidation("Content-Type", "text/plain; charset=utf-8");

            return httpRequestMessage;
        }

        private static string SerializeJsonObject(object jsonObject) => jsonObject == null ? string.Empty : JsonConvert.SerializeObject(jsonObject, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }, Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() } });
        public static string GetAsJson(this HttpClient _, object obj) => SerializeJsonObject(obj);

        public static HttpRequestMessage WithByteArrayContent(this HttpRequestMessage httpRequestMessage, byte[] bytes)
        {
            httpRequestMessage.Content = new ByteArrayContent(bytes);

            return httpRequestMessage;
        }

        public static HttpRequestMessage WithFormContent(this HttpRequestMessage httpRequestMessage, Dictionary<string, string> formDictionary)
        {
            httpRequestMessage.Content = new FormUrlEncodedContent(formDictionary);

            return httpRequestMessage;
        }

        public static HttpRequestMessage WithFormContent(this HttpRequestMessage httpRequestMessage, MultipartFormDataContent multipartFormDataContent)
        {
            httpRequestMessage.Content = multipartFormDataContent;
            return httpRequestMessage;
        }
        public static HttpRequestMessage WithContent(this HttpRequestMessage httpRequestMessage, HttpContent content)
        {
            httpRequestMessage.Content = content;
            return httpRequestMessage;
        }

        public static HttpRequestMessage WithContentType(this HttpRequestMessage httpRequestMessage, string contentType)
        {
            httpRequestMessage?.Content?.Headers.Remove("Content-Type");
            httpRequestMessage?.Content?.Headers.TryAddWithoutValidation("Content-Type", contentType);
            return httpRequestMessage!;
        }

        public static async Task<HttpRequestException> CreateHttpRequestExceptionAsync(this HttpResponseMessage responseMessage)
        {
            var exception = new HttpRequestException($"Response status code does not indicate success: {(int)responseMessage.StatusCode} ({responseMessage.StatusCode})");
            await exception.AddDataSafeAsync("ResponseBody", async () => responseMessage.Content == null ? "" : await responseMessage.Content.ReadAsStringAsync());
            await exception.AddDataSafeAsync("RequestBody", async () => await GetRedactedRequestBodyAsync(responseMessage));
            exception.AddDataSafe("RequestHeaders", () => responseMessage.RequestMessage?.Headers.RedactHeaders() ?? "");
            exception.AddDataSafe("ResponseHeaders", () => responseMessage.Headers.RedactHeaders());
            exception.AddDataAttributes(responseMessage);
            return exception;

            static async Task<object> GetRedactedRequestBodyAsync(HttpResponseMessage responseMessage)
            {
                if (responseMessage?.RequestMessage?.Content == null)
                    return "";

                return await ConvertHttpContentAsync(responseMessage?.RequestMessage?.Content);

                static async Task<string> ConvertHttpContentAsync(HttpContent? httpContent) => httpContent switch
                {
                    //Order Here is important cos Content types can derive from others e.g FormUrlEncodedContent derives from ByteArrayContent.
                    FormUrlEncodedContent _ => (await httpContent.ReadAsStringAsync()).RedactUrlEncodedContent(),
                    { } hc when hc.Headers?.ContentType?.MediaType?.Contains("form-urlencoded", StringComparison.OrdinalIgnoreCase) == true => (await httpContent.ReadAsStringAsync()).RedactUrlEncodedContent(),
                    StringContent stringContent => (await stringContent.ReadAsStringAsync()).RedactJson(),
                    ByteArrayContent _ => "Byte array content removed for logging",
                    MultipartContent multipartContent => string.Join("\r\n\r\n------\r\n\r\n", await multipartContent.SelectAsync(async h => await ConvertHttpContentAsync(h))),
                    null => string.Empty,
                    _ => await httpContent.ReadAsStringAsync()
                };
            }
        }

        public static T AddDataAttributes<T>(this T exception, HttpResponseMessage httpResponseMessage) where T : Exception
        {
            exception.AddDataSafe("RequestUri", () => httpResponseMessage?.RequestMessage?.RequestUri?.ToString() ?? string.Empty)
                    .AddDataSafe("RequestMethod", () => httpResponseMessage?.RequestMessage?.Method.ToString() ?? string.Empty)
                    .AddDataSafe("ResponseStatusCode", () => ((int?)httpResponseMessage?.StatusCode)?.ToString() ?? string.Empty)
                    .AddDataSafe("ResponseReasonPhrase", () => httpResponseMessage?.ReasonPhrase ?? string.Empty)
                    ;

            return exception;
        }

        private static async Task<T> ElseThrowImplAsync<T>(HttpResponseMessage r) => throw await r.CreateHttpRequestExceptionAsync();

        private static async Task<IHttpResponseMessageFluentMiddle?> ElseIfHttpStatusCodeAsync(this Task<IHttpResponseMessageFluentStart> httpResponseMessageFluentStartTask, HttpStatusCode httpStatusCode, Func<HttpResponseMessage, Task> handler)
        {
            var httpResponseMessageFluentStart = await httpResponseMessageFluentStartTask;
            return ((IHttpResponseMessageFluent)httpResponseMessageFluentStart).AddReponseMessageHandler((r) => r.StatusCode == httpStatusCode, handler) as IHttpResponseMessageFluentMiddle;
        }
        private static async Task<IHttpResponseMessageFluentMiddle?> ElseIfHttpStatusCodeAsync(this Task<IHttpResponseMessageFluentMiddle> httpResponseMessageFluentMiddleTask, HttpStatusCode httpStatusCode, Func<HttpResponseMessage, Task> handler)
        {
            var httpResponseMessageFluentMiddle = await httpResponseMessageFluentMiddleTask;
            return ((IHttpResponseMessageFluent)httpResponseMessageFluentMiddle).AddReponseMessageHandler((r) => r.StatusCode == httpStatusCode, handler) as IHttpResponseMessageFluentMiddle;
        }
        private static async Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfHttpStatusCodeAsync<T>(this Task<IHttpResponseMessageFluentStart<T>> httpResponseMessageFluentStartTask, HttpStatusCode httpStatusCode, Func<HttpResponseMessage, Task<T>> handler)
        {
            var httpResponseMessageFluentStart = await httpResponseMessageFluentStartTask;
            return ((HttpResponseMessageFluent<T>)httpResponseMessageFluentStart).AddReponseMessageHandler((r) => r.StatusCode == httpStatusCode, handler);
        }
        private static async Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfHttpStatusCodeAsync<T>(this Task<IHttpResponseMessageFluentMiddle<T>> httpResponseMessageFluentMiddleTask, HttpStatusCode httpStatusCode, Func<HttpResponseMessage, Task<T>> handler)
        {
            var httpResponseMessageFluentMiddle = await httpResponseMessageFluentMiddleTask;
            return ((HttpResponseMessageFluent<T>)httpResponseMessageFluentMiddle).AddReponseMessageHandler((r) => r.StatusCode == httpStatusCode, handler);
        }

        private static async Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfPredicateAsync<T>(this Task<IHttpResponseMessageFluentStart<T>> httpResponseMessageFluentStartTask, Func<HttpResponseMessage, bool> predicate, Func<HttpResponseMessage, Task<T>> handler)
        {
            var httpResponseMessageFluentStart = await httpResponseMessageFluentStartTask;
            return ((HttpResponseMessageFluent<T>)httpResponseMessageFluentStart).AddReponseMessageHandler((r) => predicate(r), handler);
        }
        private static async Task<IHttpResponseMessageFluentMiddle<T?>> ElseIfPredciateAsync<T>(this Task<IHttpResponseMessageFluentMiddle<T>> httpResponseMessageFluentMiddleTask, Func<HttpResponseMessage, bool> predicate, Func<HttpResponseMessage, Task<T>> handler)
        {
            var httpResponseMessageFluentMiddle = await httpResponseMessageFluentMiddleTask;
            return ((HttpResponseMessageFluent<T>)httpResponseMessageFluentMiddle).AddReponseMessageHandler((r) => predicate(r), handler);
        }

        private static bool IsInStatus500Range(HttpResponseMessage httpResponseMessage) => (int)httpResponseMessage.StatusCode >= 499 && (int)httpResponseMessage.StatusCode <= 599;

        public static Task<IHttpResponseMessageFluentStart> IfSuccessAsync(this Task<HttpResponseMessage> httpResponseMessageTask, Func<HttpResponseMessage, Task> handler)
        {
            var httpResponseMessageFluent = new HttpResponseMessageFluent<object>(httpResponseMessageTask).AddReponseMessageHandler((r) => r.IsSuccessStatusCode, async (r) =>
            {
                await handler(r);

                return;
            }); 

            return Task.FromResult( (httpResponseMessageFluent as IHttpResponseMessageFluentStart)! );
        }

        public static async Task ElseThrowAsync(this Task<IHttpResponseMessageFluentStart> httpResponseMessageFluentStartTask)
        {
            var httpResponseMessageFluentStart = await httpResponseMessageFluentStartTask;
            await ((IHttpResponseMessageFluent)httpResponseMessageFluentStart).AddReponseMessageHandler(r => r.IsSuccessStatusCode == false, (r) => ElseThrowImplAsync<object>(r)).EvaluateResponseAsync();
        }
        public static async Task ElseThrowAsync(this Task<IHttpResponseMessageFluentMiddle> httpResponseMessageFluentMiddleTask)
        {
            var httpResponseMessageFluentMiddle = await httpResponseMessageFluentMiddleTask;
            await ((IHttpResponseMessageFluent)httpResponseMessageFluentMiddle).AddReponseMessageHandler(r => r.IsSuccessStatusCode == false, (r) => ElseThrowImplAsync<object>(r)).EvaluateResponseAsync();
        }

        public static async Task<T?> Else<T>(this Task<HttpResponseMessage> httpResponseMessageTask, Func<HttpResponseMessage, Task<T>> handler)
        {
            return await new HttpResponseMessageFluent<T>(httpResponseMessageTask).AddReponseMessageHandler((r) => true, async (r) => await handler(r)).EvaluateResponseAsync();
        }

        public static async Task<T?> Else<T>(this Task<IHttpResponseMessageFluentStart<T>> httpResponseMessageFluentMiddleTask, Func<HttpResponseMessage, Task<T>> handler)
        {
            var httpResponseMessageFluentMiddle = await httpResponseMessageFluentMiddleTask;
            return await ((HttpResponseMessageFluent<T>)httpResponseMessageFluentMiddle).AddReponseMessageHandler(r => true, async (r) => await handler(r)).EvaluateResponseAsync();
        }

        public static async Task<T?> Else<T>(this Task<IHttpResponseMessageFluentMiddle<T>> httpResponseMessageFluentMiddleTask, Func<HttpResponseMessage, Task<T>> handler)
        {
            var httpResponseMessageFluentMiddle = await httpResponseMessageFluentMiddleTask;
            return await ((HttpResponseMessageFluent<T>)httpResponseMessageFluentMiddle).AddReponseMessageHandler(r => true, async (r) => await handler(r)).EvaluateResponseAsync();
        }


    }

    public interface IHttpClientFluent { }

    public class HttpClientFluent : IHttpClientFluent
    {
        private readonly HttpClient httpClient;

        public HttpClientFluent(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            Headers = new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> Headers { get; set; }
        public Task<HttpResponseMessage> GetAsync(string uri) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri).WithHeaders(Headers));
        public Task<HttpResponseMessage> GetAsync(string uri, string content, string contentType) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri).WithHeaders(Headers).WithStringContent(content).WithContentType(contentType));
        public Task<HttpResponseMessage> GetFormAsync(string uri, Dictionary<string, string> formDictionary, string contentType) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri).WithHeaders(Headers).WithFormContent(formDictionary).WithContentType(contentType));
        public Task<HttpResponseMessage> PostAsync(string uri) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri).WithHeaders(Headers));
        public Task<HttpResponseMessage> PostAsync(string uri, HttpContent httpContent) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri).WithHeaders(Headers).WithContent(httpContent));
        public Task<HttpResponseMessage> PostStringAsync(string uri, string content) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri).WithHeaders(Headers).WithStringContent(content));
        public Task<HttpResponseMessage> PostStringAsync(string uri, string content, string contentType) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri).WithHeaders(Headers).WithStringContent(content).WithContentType(contentType));
        public Task<HttpResponseMessage> PostJsonAsync(Uri uri, object jsonObject) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri).WithHeaders(Headers).WithJsonContent(jsonObject));
        public Task<HttpResponseMessage> PostJsonAsync(string uri, object jsonObject) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri).WithHeaders(Headers).WithJsonContent(jsonObject));
        public Task<HttpResponseMessage> PostJsonAsync(string uri, object jsonObject, HttpCompletionOption httpCompletionOption) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri).WithHeaders(Headers).WithJsonContent(jsonObject), httpCompletionOption);
        public Task<HttpResponseMessage> PostFormAsync(string uri, Dictionary<string, string> formDictionary) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri).WithHeaders(Headers).WithFormContent(formDictionary));
        public Task<HttpResponseMessage> PostFormAsync(string uri, Dictionary<string, string> formDictionary, string contentType) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri).WithHeaders(Headers).WithFormContent(formDictionary).WithContentType(contentType));
        public Task<HttpResponseMessage> PostFormAsync(string uri, MultipartFormDataContent multipartFormDataContent) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri).WithHeaders(Headers).WithFormContent(multipartFormDataContent));
        public Task<HttpResponseMessage> PostBytesAsync(string uri, byte[] bytes) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri).WithHeaders(Headers).WithByteArrayContent(bytes));
        public Task<HttpResponseMessage> PostBytesAsync(string uri, byte[] bytes, string contentType) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, uri).WithHeaders(Headers).WithByteArrayContent(bytes).WithContentType(contentType));
        public Task<HttpResponseMessage> PutAsync(string uri) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Put, uri).WithHeaders(Headers));
        public Task<HttpResponseMessage> PutStringAsync(string uri, string content, string contentType) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Put, uri).WithHeaders(Headers).WithStringContent(content).WithContentType(contentType));
        public Task<HttpResponseMessage> PutJsonAsync(string uri, object jsonObject) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Put, uri).WithHeaders(Headers).WithJsonContent(jsonObject));
        public Task<HttpResponseMessage> PutFormAsync(string uri, Dictionary<string, string> formDictionary, string contentType) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Put, uri).WithHeaders(Headers).WithFormContent(formDictionary).WithContentType(contentType));
        public Task<HttpResponseMessage> PutBytesAsync(string uri, byte[] bytes) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Put, uri).WithHeaders(Headers).WithByteArrayContent(bytes));
        public Task<HttpResponseMessage> PutBytesAsync(string uri, byte[] bytes, string contentType) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Put, uri).WithHeaders(Headers).WithByteArrayContent(bytes).WithContentType(contentType));
        public Task<HttpResponseMessage> PatchAsync(string uri) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Patch, uri).WithHeaders(Headers));
        public Task<HttpResponseMessage> PatchStringAsync(string uri, string content, string contentType) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Patch, uri).WithHeaders(Headers).WithStringContent(content).WithContentType(contentType));
        public Task<HttpResponseMessage> PatchJsonAsync(string uri, object jsonObject) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Patch, uri).WithHeaders(Headers).WithJsonContent(jsonObject));
        public Task<HttpResponseMessage> PatchFormAsync(string uri, Dictionary<string, string> formDictionary, string contentType) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Patch, uri).WithHeaders(Headers).WithFormContent(formDictionary).WithContentType(contentType));
        public Task<HttpResponseMessage> PatchBytesAsync(string uri, byte[] bytes) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Patch, uri).WithHeaders(Headers).WithByteArrayContent(bytes));
        public Task<HttpResponseMessage> PatchBytesAsync(string uri, byte[] bytes, string contentType) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Patch, uri).WithHeaders(Headers).WithByteArrayContent(bytes).WithContentType(contentType));
        public Task<HttpResponseMessage> DeleteAsync(string uri) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, uri).WithHeaders(Headers));
        public Task<HttpResponseMessage> DeleteAsync(string uri, string content, string contentType) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, uri).WithHeaders(Headers).WithStringContent(content).WithContentType(contentType));
        public Task<HttpResponseMessage> DeleteFormAsync(string uri, Dictionary<string, string> formDictionary, string contentType) => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, uri).WithHeaders(Headers).WithFormContent(formDictionary).WithContentType(contentType));
    }

    public interface IHttpResponseMessageFluentStart { }
    public interface IHttpResponseMessageFluentMiddle { }
    public interface IHttpResponseMessageFluentEnd { }

    public interface IHttpResponseMessageFluentStart<T> : IHttpResponseMessageFluentStart { }
    public interface IHttpResponseMessageFluentMiddle<T> : IHttpResponseMessageFluentMiddle { }
    public interface IHttpResponseMessageFluentEnd<T> : IHttpResponseMessageFluentEnd { }
    public interface IHttpResponseMessageFluent
    {
        IHttpResponseMessageFluent AddReponseMessageHandler(Predicate<HttpResponseMessage> predicate, Func<HttpResponseMessage, Task> handler);
        Task EvaluateResponseAsync();
    }

    public class HttpResponseMessageFluent<T> : IHttpResponseMessageFluent, IHttpResponseMessageFluentStart<T>, IHttpResponseMessageFluentMiddle<T>, IHttpResponseMessageFluentEnd<T>
    {
        private readonly List<ResponseMessageHandler> responseMessageHandlers;
        private readonly List<ExceptionMessageHandler> exceptionHandlers;
        private readonly Task<HttpResponseMessage> httpResponseMessageFluentTask;

        public HttpResponseMessageFluent(Task<HttpResponseMessage> HttpResponseMessageFluentTask)
        {
            responseMessageHandlers = new List<ResponseMessageHandler>();
            exceptionHandlers = new List<ExceptionMessageHandler>();
            httpResponseMessageFluentTask = HttpResponseMessageFluentTask;
        }
        public HttpResponseMessageFluent<T?> AddReponseMessageHandler(Predicate<HttpResponseMessage> predicate, Func<HttpResponseMessage, Task<T>> handler)
        {
            responseMessageHandlers.Add(new ResponseMessageHandler { Predicate = predicate, Handler = handler });
            return this;
        }

        public HttpResponseMessageFluent<T> AddExceptionHandler(Predicate<Exception> predicate, Func<Exception, Task<T>> handler)
        {
            exceptionHandlers.Add(new ExceptionMessageHandler { Predicate = predicate, Handler = handler });
            return this;
        }


        public async Task<T?> EvaluateResponseAsync()
        {
            HttpResponseMessage? responseMessage = null;
            try
            {
                responseMessage = await httpResponseMessageFluentTask;
                var responseMessageHandler = responseMessageHandlers.FirstOrDefault(h => h.Predicate?.Invoke(responseMessage) ?? false);
                if (responseMessageHandler == null)
                {
                    throw new ArgumentException("No ResponseMessageHandler defined").AddDataAttributes(responseMessage);
                }
                var task = responseMessageHandler.Handler?.Invoke(responseMessage);
                if (task == null)
                {
                    return default;
                }
                return await task;
            }
            catch (Exception e)
            {
                //We can be here
                //1) responseMessage = await this.httpResponseMessageFluentTask errors
                //2) awaiting the responseMessageHandler.Handler i.e IfSuccessAsync () => throws...
                if (responseMessage != null)
                {
                    e.AddDataAttributes(responseMessage);
                }

                var exceptionHandler = exceptionHandlers.FirstOrDefault(h => h.Predicate?.Invoke(e) ?? false);
                if (exceptionHandler == null)
                {
                    throw;
                }

                var task = exceptionHandler.Handler?.Invoke(e);
                if (task == null)
                {
                    return default;
                }

                try
                {
                    return await task;
                }
                catch (Exception handlerFailed)
                {
                    if (e != handlerFailed)
                    {
                        throw;
                    }
                }

                throw;
            }
        }

        //public async Task<T> EvaluateResponseAsync()
        //{
        //    var responseMessage = await this.httpResponseMessageFluentTask;

        //    var responseMessageHandler = this.responseMessageHandlers.FirstOrDefault(h => h.Predicate(responseMessage));
        //    if (responseMessageHandler != null)
        //    {
        //        try
        //        {
        //            var task = responseMessageHandler.Handler(responseMessage);
        //            if(task == null)
        //            {
        //                return default;
        //            }
        //            return await task;
        //        }
        //        catch(HttpRequestException)
        //        {
        //            //Thrown by ElseThrowXXX fluent
        //            throw;
        //        }
        //        catch (Exception e)
        //        {
        //            //Thrown most likey when somethings got wrong inside a fulent call back e.g. IfSuccessAsync etc
        //            if (responseMessage != null)
        //            {
        //                e.AddDataAttributes(responseMessage);
        //            }
        //            throw;
        //        }
        //    }

        //    throw new ArgumentException("No ResponseMessageHandler defined").AddDataAttributes(responseMessage);
        //}

        Task IHttpResponseMessageFluent.EvaluateResponseAsync()
        {
            return EvaluateResponseAsync();
        }

        public IHttpResponseMessageFluent AddReponseMessageHandler(Predicate<HttpResponseMessage> predicate, Func<HttpResponseMessage, Task> handler)
        {
            AddReponseMessageHandler(predicate, async (r) =>
            {
                await handler(r);
                return default!;
            });

            return this;
        }

        private class ResponseMessageHandler
        {
            public Predicate<HttpResponseMessage>? Predicate { get; set; }
            public Func<HttpResponseMessage, Task<T>>? Handler { get; set; }
        }

        private class ExceptionMessageHandler
        {
            public Predicate<Exception>? Predicate { get; set; }
            public Func<Exception, Task<T>>? Handler { get; set; }
        }
    }
}
