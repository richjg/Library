using System.Net;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Library;

namespace LibraryTests
{
    public class HttpClientExtensionsTests
    {
        [Test]
        public async Task CreateHttpRequestExceptionAsync_IncludesMessage()
        {
            // arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            // act
            var ex = await response.CreateHttpRequestExceptionAsync();

            // assert
            Assert.That(ex.Message, Is.EqualTo($"Response status code does not indicate success: 400 (BadRequest)"));
        }

        [Test]
        public async Task CreateHttpRequestExceptionAsync_IncludesRequestContent_WhenItsJson()
        {
            // arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://test")
                {
                    Content = new StringContent(new { data = 1 }.ToJson()),
                }
            };

            // act
            var ex = await response.CreateHttpRequestExceptionAsync();

            // assert
            Assert.That(ex.Data["RequestBody"], Is.EqualTo(@"{
  ""data"": 1
}"));
        }

        [Test]
        public async Task CreateHttpRequestExceptionAsync_IncludesRequestContent_WhenItsNotJson()
        {
            // arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://test")
                {
                    Content = new StringContent("Blah"),
                }
            };

            // act
            var ex = await response.CreateHttpRequestExceptionAsync();

            // assert
            var requestBody = ex.Data["RequestBody"] as string;
            Assert.That(requestBody, Is.EqualTo("Blah"));
        }

        [Test]
        public async Task CreateHttpRequestExceptionAsync_DoesNotLogTheByteArray_WhenRequestContentIsAByteArray()
        {
            // arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://test")
                {
                    Content = new ByteArrayContent(new byte[] { 0x01, 0x02, 0x03 })
                }
            };

            // act
            var ex = await response.CreateHttpRequestExceptionAsync();

            // assert
            var requestBody = ex.Data["RequestBody"] as string;
            Assert.That(requestBody, Is.EqualTo("Byte array content removed for logging"));
        }

        [Test]
        public async Task CreateHttpRequestExceptionAsync_IncludesResponseContent_WhenItsJson()
        {
            // arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(new { data = 1 }.ToJson()),
            };

            // act
            var ex = await response.CreateHttpRequestExceptionAsync();

            // assert
            Assert.That(ex.Data["ResponseBody"], Is.EqualTo(@"{""data"":1}"));
        }

        [Test]
        public async Task CreateHttpRequestExceptionAsync_IncludesResponseContent_WhenItsNotJson()
        {
            // arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Blah"),
            };

            // act
            var ex = await response.CreateHttpRequestExceptionAsync();

            // assert
            var responseBody = ex.Data["ResponseBody"] as string;
            Assert.That(responseBody, Is.EqualTo("Blah"));
        }

        [Test]
        public async Task CreateHttpRequestExceptionAsync_IncludesRequestHeaders()
        {
            // arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test")
                {
                    Headers = { { "heading", "value1" } }
                }
            };

            // act
            var ex = await response.CreateHttpRequestExceptionAsync();

            // assert
            Assert.That(ex.Data["RequestHeaders"], Is.EqualTo(@"{
  ""heading"": [
    ""value1""
  ]
}"));
        }

        [Test]
        public async Task CreateHttpRequestExceptionAsync_IncludesResponseHeaders()
        {
            // arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Headers = { { "heading", "value1" } }
            };

            // act
            var ex = await response.CreateHttpRequestExceptionAsync();

            // assert
            Assert.That(ex.Data["ResponseHeaders"], Is.EqualTo(@"{
  ""heading"": [
    ""value1""
  ]
}"));
        }

        [Test]
        public void CreateHttpRequestExceptionAsync_DoesNotThrow_WhenNoRequest()
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

            Assert.DoesNotThrowAsync(async () =>
            {
                var ex = await response.CreateHttpRequestExceptionAsync();
            });
        }

        [Test]
        public async Task CreateHttpRequestExceptionAsync_RedactsSensitiveData_WhenContentIsAMultipartFormDataContent()
        {
            // arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://test")
                {
                    Content = new MultipartFormDataContent
                    {
                        new ByteArrayContent(new byte[] { 0x01, 0x02, 0x03 }),
                        new StringContent(new Dictionary<string, object> { ["data"] = 1, ["password"] = "password" }.ToJson()),
                        new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("client_id", "id1"),
                            new KeyValuePair<string, string>("client_secret", "secret")
                        })
                    }
                }
            };

            // act
            var ex = await response.CreateHttpRequestExceptionAsync();
            // assert
            Assert.That(ex.Data["RequestBody"], Is.EqualTo(@"Byte array content removed for logging

------

{
  ""data"": 1,
  ""password"": ""**Redacted**""
}

------

{
  ""client_id"": ""id1"",
  ""client_secret"": ""**Redacted**""
}"));
        }

        [TestCase("pass")]
        [TestCase("PaSs")]
        [TestCase("password")]
        [TestCase("thepasswordis")]
        [TestCase("secret")]
        [TestCase("client_secret")]
        public async Task CreateHttpRequestExceptionAsync_RedactsSensitiveData_WhenRequestContentIsJsonWithSensitiveContent(string propertyName)
        {
            // arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://test")
                {
                    Content = new StringContent(new Dictionary<string, object> { ["data"] = 1, [propertyName] = "password" }.ToJson()),
                }
            };

            //act
            var ex = await response.CreateHttpRequestExceptionAsync();

            //assert
            Assert.That(ex.Data["RequestBody"], Is.EqualTo(@$"{{
  ""data"": 1,
  ""{propertyName}"": ""**Redacted**""
}}"));
        }

        [Test]
        public async Task CreateHttpRequestExceptionAsync_RedactsSensitiveData_WhenRequestContentIsFormUrlEncoded()
        {
            // arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://test")
                {
                    Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("client_id", "id1"),
                        new KeyValuePair<string, string>("client_secret", "secret")
                    })
                }
            };

            // act
            var ex = await response.CreateHttpRequestExceptionAsync();

            // assert
            Assert.That(ex.Data["RequestBody"], Is.EqualTo(@$"{{
  ""client_id"": ""id1"",
  ""client_secret"": ""**Redacted**""
}}"));
        }

        [Test]
        public async Task CreateHttpRequestExceptionAsync_RedactsSensitiveData_WhenRequestContentIsFormUrlEncodedWithDuplicateKeys()
        {
            // arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://test")
                {
                    Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("key1", "id1"),
                        new KeyValuePair<string, string>("key1", "id2"),
                        new KeyValuePair<string, string>("passwords", "123"),
                        new KeyValuePair<string, string>("passwords", "456"),
                    })
                }
            };

            // act
            var ex = await response.CreateHttpRequestExceptionAsync();

            // assert
            Assert.That(ex.Data["RequestBody"], Is.EqualTo(@"{
  ""key1"": ""id1,id2"",
  ""passwords"": ""**Redacted**""
}"));
        }

        [TestCase("Basic")]
        [TestCase("Ocp-Apim-Subscription-Key")]
        [TestCase("EndpointKey")]
        [TestCase("api-key")]
        [TestCase("Bearer")]
        public async Task CreateHttpRequestExceptionAsync_RedactsSensitiveData_WhenRequestHeaderContainsAuthorization(string scheme)
        {
            // arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test")
                {
                    Headers = { { "Authorization", $"{scheme} auth-value-here" } }
                }
            };

            // act
            var ex = await response.CreateHttpRequestExceptionAsync();

            // assert
            Assert.That(ex.Data["RequestHeaders"], Is.EqualTo(@"{
  ""Authorization"": [
    ""**Redacted**""
  ]
}"));
        }

        [Test]
        public void HttpClient_GetAsJson_ReturnsJsonStringWithoutDollarType()
        {
            var obj = new
            {
                number = 1,
                obj = (object)new
                {
                    text = "text"
                }
            };
            var client = new HttpClient();

            //act
            var result = client.GetAsJson(obj);

            //assert
            Assert.That(result, Is.EqualTo("{\"number\":1,\"obj\":{\"text\":\"text\"}}"));
        }

        private enum TestEnum
        {
            One,
            Two,
        }

        private class TestClass
        {
            public TestEnum TestEnum { get; set; }
        }

        [Test]
        public async Task CreateHttpRequest_EnumsAreConvertedToTheirStringValue()
        {
            // arrange
            var clientHandlerFake = new HttpClientHandlerFake { HttpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) };
            var client = new HttpClient(clientHandlerFake);

            // act
            await client.WithoutHeader().PostJsonAsync("http://example.com", new TestClass { TestEnum = TestEnum.One })
                            .IfSuccessAsync(r => Task.CompletedTask);

            // assert
            Assert.That(clientHandlerFake.RequestContent, Is.EqualTo(@"{""testEnum"":""One""}"));
        }

        [Test]
        public async Task IfSuccessAsync_ReturnsDataWhenStatusOk()
        {
            // arrange
            var clientHandlerFake = new FakeFuncHttpMessageHandler
            {
                HandleRequest = (r) =>
                {
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new { result = 1 }.ToJson()) };
                }
            };
            var client = new HttpClient(clientHandlerFake);

            // act
            var data = await client.WithoutHeader()
                            .GetAsync("http://example.com")
                            .IfSuccessAsync(r => r.ContentFromJsonAsync<JObject>())
                            .ElseThrowAsync();

            // assert
            Assert.That((int)data!["result"]!, Is.EqualTo(1));
        }

        [Test]
        public async Task ElseIfStatusCodeAsync_Called_WhenStatusCode([Values] HttpStatusCode httpStatusCode)
        {
            // arrange
            var clientHandlerFake = new FakeFuncHttpMessageHandler
            {
                HandleRequest = (r) =>
                {
                    return new HttpResponseMessage(httpStatusCode) { Content = new StringContent(new { result = 1 }.ToJson()) };
                }
            };
            var client = new HttpClient(clientHandlerFake);

            // act
            var data = await client.WithoutHeader()
                            .GetAsync("http://example.com")
                            .IfSuccessAsync(r => r.StatusCode.AsCompletedTask())
                            .ElseIfStatusCodeAsync(httpStatusCode, r => r.StatusCode.AsCompletedTask())
                            .ElseThrowAsync();

            // assert
            Assert.That(data, Is.EqualTo(httpStatusCode));
        }

        [Test]
        public async Task ElseIfBadRequestAsync_Called_WhenStatusCodeBadRequest()
        {
            // arrange
            var clientHandlerFake = new FakeFuncHttpMessageHandler
            {
                HandleRequest = (r) =>
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(new { result = 1 }.ToJson()) };
                }
            };
            var client = new HttpClient(clientHandlerFake);

            // act
            var data = await client.WithoutHeader()
                            .GetAsync("http://example.com")
                            .IfSuccessAsync(r => r.StatusCode.AsCompletedTask())
                            .ElseIfBadRequestAsync(r => r.StatusCode.AsCompletedTask())
                            .ElseThrowAsync();

            // assert
            Assert.That(data, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task ElseIfConflictAsync_Called_WhenStatusCodeConfilct()
        {
            // arrange
            var clientHandlerFake = new FakeFuncHttpMessageHandler
            {
                HandleRequest = (r) =>
                {
                    return new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent(new { result = 1 }.ToJson()) };
                }
            };
            var client = new HttpClient(clientHandlerFake);

            // act
            var data = await client.WithoutHeader()
                            .GetAsync("http://example.com")
                            .IfSuccessAsync(r => r.StatusCode.AsCompletedTask())
                            .ElseIfConflictAsync(r => r.StatusCode.AsCompletedTask())
                            .ElseThrowAsync();

            // assert
            Assert.That(data, Is.EqualTo(HttpStatusCode.Conflict));
        }

        [Test]
        public async Task ElseIfIn500RangeRequestAsync_Called_WhenStatusIn500([Range(499, 599)] int value)
        {
            // #4538 Tweak to 500 range error to include the non-standard code 499.
            // As best can tell a 499 is a variation of a 502 bad gateway
            // From the point of view of error handling, want to treat it like the other 500 errors.

            // arrange
            var clientHandlerFake = new FakeFuncHttpMessageHandler
            {
                HandleRequest = (r) =>
                {
                    return new HttpResponseMessage((HttpStatusCode)value) { Content = new StringContent(new { result = 1 }.ToJson()) };
                }
            };
            var client = new HttpClient(clientHandlerFake);

            // act
            var data = await client.WithoutHeader()
                            .GetAsync("http://example.com")
                            .IfSuccessAsync(r => r.StatusCode.AsCompletedTask())
                            .ElseIfIn500RangeRequestAsync(r => r.StatusCode.AsCompletedTask())
                            .ElseThrowAsync();

            // assert
            Assert.That(data, Is.EqualTo((HttpStatusCode)value));
        }

        [Test]
        public async Task ElseIfNotFoundAsync_Called_WhenStatusNotFound()
        {
            // arrange
            var clientHandlerFake = new FakeFuncHttpMessageHandler
            {
                HandleRequest = (r) =>
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent(new { result = 1 }.ToJson()) };
                }
            };
            var client = new HttpClient(clientHandlerFake);

            // act
            var data = await client.WithoutHeader()
                            .GetAsync("http://example.com")
                            .IfSuccessAsync(r => r.StatusCode.AsCompletedTask())
                            .ElseIfNotFoundAsync(r => r.StatusCode.AsCompletedTask())
                            .ElseThrowAsync();

            // assert
            Assert.That(data, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task ElseIfTooManyRequestsAsync_Called_WhenStatusToManyRequests()
        {
            // arrange
            var clientHandlerFake = new FakeFuncHttpMessageHandler
            {
                HandleRequest = (r) =>
                {
                    return new HttpResponseMessage(HttpStatusCode.TooManyRequests) { Content = new StringContent(new { result = 1 }.ToJson()) };
                }
            };
            var client = new HttpClient(clientHandlerFake);

            // act
            var data = await client.WithoutHeader()
                            .GetAsync("http://example.com")
                            .IfSuccessAsync(r => r.StatusCode.AsCompletedTask())
                            .ElseIfTooManyRequestsAsync(r => r.StatusCode.AsCompletedTask())
                            .ElseThrowAsync();

            // assert
            Assert.That(data, Is.EqualTo(HttpStatusCode.TooManyRequests));
        }

        [Test]
        public async Task ElseIfUnauthorizedAsync_Called_WhenStatusUnAouthorized()
        {
            // arrange
            var clientHandlerFake = new FakeFuncHttpMessageHandler
            {
                HandleRequest = (r) =>
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent(new { result = 1 }.ToJson()) };
                }
            };
            var client = new HttpClient(clientHandlerFake);

            // act
            var data = await client.WithoutHeader()
                            .GetAsync("http://example.com")
                            .IfSuccessAsync(r => r.StatusCode.AsCompletedTask())
                            .ElseIfUnauthorizedAsync(r => r.StatusCode.AsCompletedTask())
                            .ElseThrowAsync();

            // assert
            Assert.That(data, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public void ElseThrowAsync_Called_WhenServerThrowsAnError()
        {
            // arrange
            var clientHandlerFake = new FakeFuncHttpMessageHandler
            {
                HandleRequest = (r) =>
                {
                    throw new Exception("Server Error");
                }
            };
            var client = new HttpClient(clientHandlerFake);

            // act
            async Task Act()
            {
                var data = await client.WithoutHeader()
                                .GetAsync("http://example.com")
                                .IfSuccessAsync(r => r.StatusCode.AsCompletedTask())
                                .ElseThrowAsync();
            }

            var ex = Assert.ThrowsAsync<Exception>(Act);

            // assert
            Assert.That(ex.Message, Is.EqualTo("Server Error"));
        }

        [Test]
        public void ElseThrowAsync_Called_WhenReturnStatusNotOK()
        {
            // arrange
            var clientHandlerFake = new FakeFuncHttpMessageHandler
            {
                HandleRequest = (r) =>
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(new { result = 1 }.ToJson()) };
                }
            };
            var client = new HttpClient(clientHandlerFake);

            // act
            async Task Act()
            {
                await client.WithoutHeader()
                                .GetAsync("http://example.com")
                                .IfSuccessAsync(r => true.AsCompletedTask())
                                .ElseThrowAsync();
            }

            var ex = Assert.ThrowsAsync<HttpRequestException>(Act);

            // assert
            Assert.That(ex.Message, Is.EqualTo("Response status code does not indicate success: 400 (BadRequest)"));
        }

        [Test]
        public void ElseThrowAsync_Called_WhenGettingTheIfSuccessAsyncLambdaThrows()
        {
            // arrange
            var clientHandlerFake = new FakeFuncHttpMessageHandler
            {
                HandleRequest = (r) =>
                {
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new { result = 1 }.ToJson()) };
                }
            };
            var client = new HttpClient(clientHandlerFake);

            // act
            async Task Act()
            {
                await client.WithoutHeader()
                                .GetAsync("http://example.com")
                                .IfSuccessAsync(r => throw new Exception("Getting Data Error"))
                                .ElseThrowAsync();
            }

            var ex = Assert.ThrowsAsync<Exception>(Act);

            // assert
            Assert.That(ex.Message, Is.EqualTo("Getting Data Error"));
        }

        [Test]
        public async Task ElseIfTimeoutAsync_CatchesATaskCancelledException()
        {
            // **** see Polly test for example of catch a TimeoutRejectedException ****

            // arrange
            var clientHandlerFake = new FakeFuncHttpMessageHandler
            {
                HandleRequestAsync = async (r) =>
                {
                    await Task.Yield();
                    await Task.Delay(300);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new { result = 1 }.ToJson()) };
                }
            };
            var client = new HttpClient(clientHandlerFake);
            client.Timeout = TimeSpan.FromMilliseconds(1);

            var result = await client.WithoutHeader()
                                .GetAsync("http://example.com")
                                .IfSuccessAsync(r => false.AsCompletedTask())
                                .ElseIfTimeoutAsync(() => true.AsCompletedTask())
                                .ElseThrowAsync();


            // assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public async Task ElseIfTaskCancelledExceptionAsync_Called_WhenIfSuccessAsyncLambdaThrowsTaskCancelledException()
        {
            // arrange
            var clientHandlerFake = new FakeFuncHttpMessageHandler
            {
                HandleRequestAsync = async (r) =>
                {
                    await Task.Delay(10);
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new { result = 1 }.ToJson()) };
                }
            };
            var client = new HttpClient(clientHandlerFake);
            client.Timeout = TimeSpan.FromMilliseconds(1);

            var result = await client.WithoutHeader()
                                .GetAsync("http://example.com")
                                .IfSuccessAsync(r =>
                                {
                                    int i = 0;
                                    if (i == 0)
                                    {
                                        throw new TaskCanceledException();
                                    }

                                    return false.AsCompletedTask();
                                })
                                .ElseIfTimeoutAsync(() => true.AsCompletedTask())
                                .ElseThrowAsync();


            // assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public async Task ElseIfHttpRequestExceptionAsync_Called_WhenReturnStatusNotOk()
        {
            // arrange
            var clientHandlerFake = new FakeFuncHttpMessageHandler
            {
                HandleRequest = (r) =>
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(new { result = 1 }.ToJson()) };
                }
            };
            var client = new HttpClient(clientHandlerFake);

            // act
            var result = await client.WithoutHeader()
                                           .GetAsync("http://example.com")
                                           .IfSuccessAsync(r => false.AsCompletedTask())
                                           .ElseIfHttpRequestExceptionAsync(e => true.AsCompletedTask())
                                           .ElseThrowAsync();

            // assert
            Assert.That(result, Is.EqualTo(true));
        }
    }
}
