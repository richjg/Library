using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.Core;

namespace LibraryTests
{
    [TestFixture]
    public class PollyTests
    {
        public class CountingMessageHandler : HttpMessageHandler
        {
            public Func<HttpRequestMessage, CancellationToken, int, HttpResponseMessage>? HandleRequest { get; set; }
            public Func<HttpRequestMessage, CancellationToken, int, Task<HttpResponseMessage>>? HandleRequestAsync { get; set; }
            public int TryCount { get; private set; } = 0;

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                try
                {
                    if (HandleRequest != null)
                        return HandleRequest(request, cancellationToken, TryCount);
                    else if (HandleRequestAsync != null)
                        return await HandleRequestAsync(request, cancellationToken, TryCount);
                    else
                        throw new ArgumentException("CountingMessageHandler: You must define either HandleRequest or HandleRequestAsync");
                }
                finally
                {
                    this.TryCount++;
                }
            }
        }

        public class HttpPolicyLoggerFake : IHttpPolicyLogger
        {
            public List<object> Events = new List<object>();

            public Task SendAsync(object customEvent)
            {
                this.Events.Add(customEvent);
                return Task.CompletedTask;
            }
        }


        [TestCase(HttpStatusCode.RequestTimeout)]       // 408
        [TestCase(HttpStatusCode.TooManyRequests)]      // 429
        [TestCase(HttpStatusCode.InternalServerError)]  // 500
        [TestCase(HttpStatusCode.BadGateway)]           // 502
        [TestCase(HttpStatusCode.ServiceUnavailable)]   // 503
        [TestCase(HttpStatusCode.GatewayTimeout)]       // 504 
        public async Task DefaultPolicy_WillRetry_IfHttpStatusCodeInTheList(HttpStatusCode statusCode)
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequest = (request, ctx, count) =>
                {
                    if (count == 0)
                        return new HttpResponseMessage(statusCode);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            bool result = await httpClient
                    .GetAsync("http://example.com")
                    .IfSuccessAsync(r => true.AsCompletedTask())
                    .ElseThrowAsync();

            //Assert
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(2));
            Assert.That(result, Is.True);
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().RetryReason, Is.EqualTo("ResponseStatusCode"));
        }

        [Test]
        public void DefaultPolicy_WillRetry3Times_BeforeThrowing()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequest = (request, ctx, count) =>
                {
                    return new HttpResponseMessage(HttpStatusCode.BadGateway);
                }
            };
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            async Task Act()
            {
                await httpClient
                    // this website does not exist (it's a random guid), so the inner exception is a SocketException - No such host is known.
                    .GetAsync("http://example.com")
                    .IfSuccessAsync(r => "".AsCompletedTask())
                    .ElseThrowAsync();
            }
            var exception = Assert.ThrowsAsync<HttpRequestException>(Act);

            //Assert
            Assert.That(exception, Is.TypeOf<HttpRequestException>());

            Assert.That(httpClientHandler.TryCount, Is.EqualTo(4));    // This is 4 not 3 because the first call is not a retry.
            Assert.That(exception.Data["ResponseStatusCode"], Is.EqualTo("502"));
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task DefaultPolicy_OnA429_IfRetryAfterIsNull_WillUseDefaultRetryDelay()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequest = (request, ctx, count) =>
                {
                    if (count == 0)
                        return new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };

            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            bool result = await httpClient
                .GetAsync("http://example.com")
                .IfSuccessAsync(r => true.AsCompletedTask())
                .ElseThrowAsync();

            //Assert
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(2));
            Assert.That(result, Is.True);
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().RetryReason, Is.EqualTo("ResponseStatusCode"));
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().ResponseStatusCode, Is.EqualTo(429));
            TimeSpan retryDelay = httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().RetryDelay;
            Assert.That(retryDelay, Is.EqualTo(TimeSpan.FromSeconds(4)));
        }

        [Test]
        public async Task DefaultPolicy_OnA429_IfRetryAfterIsLessThanOrEqualTo10Seconds_ItWillBeUsed()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequest = (request, ctx, count) =>
                {
                    if (count == 0)
                        return new HttpResponseMessage(HttpStatusCode.TooManyRequests).AddHeader("Retry-After", "1");
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };

            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            bool result = await httpClient
                .GetAsync("http://example.com")
                .IfSuccessAsync(r => true.AsCompletedTask())
                .ElseThrowAsync();

            //Assert
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(2));
            Assert.That(result, Is.True);
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().RetryReason, Is.EqualTo("ResponseStatusCode"));
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().ResponseStatusCode, Is.EqualTo(429));
            TimeSpan retryDelay = httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().RetryDelay;
            Assert.That(retryDelay, Is.EqualTo(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public async Task DefaultPolicy_OnA429_IfRetryAfterIsGreaterThan20Seconds_20SecondsWillBeUsed()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequest = (request, ctx, count) =>
                {
                    if (count == 0)
                        return new HttpResponseMessage(HttpStatusCode.TooManyRequests).AddHeader("Retry-After", "21");
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };

            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            bool result = await httpClient
                .GetAsync("http://example.com")
                .IfSuccessAsync(r => true.AsCompletedTask())
                .ElseThrowAsync();

            //Assert
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(2));
            Assert.That(result, Is.True);
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().RetryReason, Is.EqualTo("ResponseStatusCode"));
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().ResponseStatusCode, Is.EqualTo(429));
            TimeSpan retryDelay = httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().RetryDelay;
            Assert.That(retryDelay, Is.EqualTo(TimeSpan.FromSeconds(20)));
        }

        [Test]
        public void DefaultPolicy_OnA429_WillRetry15Times_BeforeThrowing()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequest = (request, ctx, count) =>
                {
                    return new HttpResponseMessage(HttpStatusCode.TooManyRequests).AddHeader("Retry-After", "0");
                }
            };

            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            async Task Act()
            {
                await httpClient
                    .GetAsync("http://example.com")
                    .IfSuccessAsync(r => "".AsCompletedTask())
                    .ElseThrowAsync();
            }
            var exception = Assert.ThrowsAsync<HttpRequestException>(Act);

            //Assert
            Assert.That(exception, Is.TypeOf<HttpRequestException>());

            Assert.That(httpClientHandler.TryCount, Is.EqualTo(16));    // First call is not a retry.
            Assert.That(exception.Data["ResponseStatusCode"], Is.EqualTo("429"));
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(15));
        }

        [Test]
        [Ignore("Real")]
        public void Real_DefaultPolicy_On429_WillThrow_AndLogInfoEventToTelemetry()
        {
            //Arrange
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake) { RetryDelays = new() { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero } }.CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = new SocketsHttpHandler() });

            //Act
            async Task Act()
            {
                await httpClient
                    // This website is great, and returns the status code from the url.
                    .GetAsync("http://httpstat.us/429")
                    .IfSuccessAsync(r => "".AsCompletedTask())
                    .ElseThrowAsync();
            }
            var exception = Assert.ThrowsAsync<HttpRequestException>(Act);

            //Assert
            Assert.That(exception, Is.TypeOf<HttpRequestException>());
            Assert.That(exception.Message, Is.EqualTo("Response status code does not indicate success: 429 (TooManyRequests)"));

            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(15));
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().RetryReason, Is.EqualTo("ResponseStatusCode"));
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().ResponseStatusCode, Is.EqualTo(429));
        }

        [Test]
        public async Task DefaultPolicy_OnA429_WillRetry15Times_WontThrowIfUsingElseIfTooManyRequestsExtension()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequest = (request, ctx, count) =>
                {
                    return new HttpResponseMessage(HttpStatusCode.TooManyRequests).AddHeader("Retry-After", "0");
                }
            };
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });
            var tooManyRequestsOccured = false;

            //Act
            var result = await httpClient
                    .GetAsync("http://example.com")
                    .IfSuccessAsync(r => "".AsCompletedTask())
                    .ElseIfTooManyRequestsAsync(r => { tooManyRequestsOccured = true; return "TooManyRequestsOccured".AsCompletedTask(); })
                    .ElseThrowAsync();

            //Assert
            Assert.That(tooManyRequestsOccured, Is.True);
            Assert.That(result, Is.EqualTo("TooManyRequestsOccured"));
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(16));    // First call is not a retry.
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(15));
        }

        [Test]
        public async Task DefaultPolicy_RetrysCorrectly_WhenAMixtureOfStatusCodes()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequest = (request, ctx, count) =>
                {
                    if (count <= 3)
                        return new HttpResponseMessage(HttpStatusCode.TooManyRequests).AddHeader("Retry-After", "0");

                    if (count == 4)
                        return new HttpResponseMessage(HttpStatusCode.RequestTimeout);

                    if (count == 5)
                        return new HttpResponseMessage(HttpStatusCode.TooManyRequests).AddHeader("Retry-After", "0");

                    if (count == 6)
                        return new HttpResponseMessage(HttpStatusCode.TooManyRequests).AddHeader("Retry-After", "0");

                    if (count == 7)
                        return new HttpResponseMessage(HttpStatusCode.BadGateway);

                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake) { RetryDelays = new() { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero } }.CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            bool result = await httpClient
                .GetAsync("http://example.com")
                .IfSuccessAsync(r => true.AsCompletedTask())
                .ElseThrowAsync();

            //Assert
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(9)); // First call is not a retry.
            Assert.That(result, Is.True);
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(8));
        }

        [Test]
        public void DefaultPolicy_Throws_After429RetryLimitExceeds_WhenMixedWithOtherStatusCodes()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequest = (request, ctx, count) =>
                {
                    if (count <= 10)
                        return new HttpResponseMessage(HttpStatusCode.TooManyRequests).AddHeader("Retry-After", "0");

                    if (count == 11)
                        return new HttpResponseMessage(HttpStatusCode.RequestTimeout);

                    if (count >= 12)
                        return new HttpResponseMessage(HttpStatusCode.TooManyRequests).AddHeader("Retry-After", "0");

                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake) { RetryDelays = new() { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero } }.CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            async Task Act()
            {
                await httpClient
                    .GetAsync("http://example.com")
                    .IfSuccessAsync(r => "".AsCompletedTask())
                    .ElseThrowAsync();
            }
            var exception = Assert.ThrowsAsync<HttpRequestException>(Act);

            //Assert
            Assert.That(exception, Is.TypeOf<HttpRequestException>());

            Assert.That(httpClientHandler.TryCount, Is.EqualTo(17));    // First call is not a retry.
            Assert.That(exception.Data["ResponseStatusCode"], Is.EqualTo("429"));
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(16));
        }

        [Test]
        public void DefaultPolicy_Throws_AfterRetryLimitExceeded_WhenMixedWith429()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequest = (request, ctx, count) =>
                {
                    if (count == 0)
                        return new HttpResponseMessage(HttpStatusCode.TooManyRequests).AddHeader("Retry-After", "0");

                    if (count == 1)
                        return new HttpResponseMessage(HttpStatusCode.RequestTimeout);

                    if (count == 2)
                        return new HttpResponseMessage(HttpStatusCode.TooManyRequests).AddHeader("Retry-After", "0");

                    return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
                }
            };
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake) { RetryDelays = new() { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero } }.CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            async Task Act()
            {
                await httpClient
                    .GetAsync("http://example.com")
                    .IfSuccessAsync(r => "".AsCompletedTask())
                    .ElseThrowAsync();
            }
            var exception = Assert.ThrowsAsync<HttpRequestException>(Act);

            //Assert
            Assert.That(exception, Is.TypeOf<HttpRequestException>());

            Assert.That(httpClientHandler.TryCount, Is.EqualTo(7));    // First call is not a retry.
            Assert.That(exception.Data["ResponseStatusCode"], Is.EqualTo("408"));
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(6));
        }

        [Test]
        public void DefaultPolicy_WillNotRetry_OnOther400RangeErrors()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequest = (request, ctx, count) =>
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            };
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            async Task Act()
            {
                await httpClient
                    .GetAsync("http://example.com")
                    .IfSuccessAsync(r => true.AsCompletedTask())
                    .ElseThrowAsync();
            }
            var exception = Assert.ThrowsAsync<HttpRequestException>(Act);

            //Assert
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(1));
            Assert.That(exception.Data["ResponseStatusCode"], Is.EqualTo("400"));
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task DefaultPolicy_WillRetry_SocketExceptions()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequestAsync = (request, ctx, count) =>
                {
                    if (count == 0)
                    {
                        // Simulate a socket exception
                        var socketException = new SocketException((int)SocketError.HostNotFound);
                        return Task.FromException<HttpResponseMessage>(new HttpRequestException($"{socketException.Message} ({request?.RequestUri?.Host}:{request?.RequestUri?.Port})", socketException));
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK).AsCompletedTask();
                }
            };
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            bool result = await httpClient
                .GetAsync("http://example.com")
                .IfSuccessAsync(r => true.AsCompletedTask())
                .ElseThrowAsync();

            //Assert
            Assert.That(result, Is.True);
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(2));
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().RetryReason, Is.EqualTo("SocketException"));
        }

        [Test]
        public void DefaultPolicy_WillRetry_SocketExceptions_RealButQuick()
        {
            //Arrange
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake) { RetryDelays = new() { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero } }.CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = new SocketsHttpHandler() });

            //Act
            async Task Act()
            {
                await httpClient
                    // this website does not exist (it's a random guid), so the inner exception is a SocketException - No such host is known.
                    .GetAsync("http://ECB43F4B-AC4B-4066-9C90-E2806BDC25B2.com")
                    .IfSuccessAsync(r => "".AsCompletedTask())
                    .ElseThrowAsync();
            }
            var exception = Assert.ThrowsAsync<HttpRequestException>(Act);

            //Assert
            Assert.That(exception, Is.TypeOf<HttpRequestException>());
            Assert.That((exception.InnerException as SocketException)?.SocketErrorCode, Is.EqualTo(SocketError.HostNotFound));

            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(3));
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().RetryReason, Is.EqualTo("SocketException"));
        }

        [Test]
        public void DefaultPolicy_WillNotRetry_CertificateErrors()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequestAsync = (request, ctx, count) =>
                {
                    if (count == 0)
                    {
                        // Simulate a authentication exception
                        var authenticationException = new AuthenticationException("The remote certificate is invalid because of errors in the certificate chain: NotTimeValid");
                        return Task.FromException<HttpResponseMessage>(new HttpRequestException($"The SSL connection could not be established, see inner exception.", authenticationException));
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK).AsCompletedTask();
                }
            };
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            async Task Act()
            {
                await httpClient
                    .GetAsync("http://example.com")
                    .IfSuccessAsync(r => true.AsCompletedTask())
                    .ElseThrowAsync();
            }
            var exception = Assert.ThrowsAsync<HttpRequestException>(Act);

            //Assert
            Assert.That(exception, Is.TypeOf<HttpRequestException>());
            Assert.That(exception.InnerException, Is.TypeOf<AuthenticationException>());
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(1));
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(0));
        }

        [Test]
        public void DefaultPolicy_WillNotRetry_CertificateError_RealButQuick()
        {
            //Arrange
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = new SocketsHttpHandler() });

            //Act
            async Task Act()
            {
                await httpClient
                    // this is a test website with a bunch of different certificate errors on it
                    .GetAsync("https://expired.badssl.com/")
                    .IfSuccessAsync(r => "".AsCompletedTask())
                    .ElseThrowAsync();
            }
            var exception = Assert.ThrowsAsync<HttpRequestException>(Act);

            //Assert
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(0));
            Assert.That(exception, Is.TypeOf<HttpRequestException>());
            Assert.That(exception.InnerException, Is.TypeOf<AuthenticationException>());
        }

        [Test]
        public void DefaultPolicy_WillNotRetry_OnASelfSignedCertificate()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequestAsync = (request, ctx, count) =>
                {
                    if (count == 0)
                    {
                        // Simulate a self signed certificate
                        var authenticationException = new AuthenticationException("The remote certificate is invalid because of errors in the certificate chain: UntrustedRoot");
                        return Task.FromException<HttpResponseMessage>(new HttpRequestException($"The SSL connection could not be established, see inner exception.", authenticationException));
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK).AsCompletedTask();
                }
            };
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            async Task Act()
            {
                await httpClient
                    .GetAsync("http://example.com")
                    .IfSuccessAsync(r => true.AsCompletedTask())
                    .ElseThrowAsync();
            }
            var exception = Assert.ThrowsAsync<HttpRequestException>(Act);

            //Assert
            Assert.That(exception, Is.TypeOf<HttpRequestException>());
            Assert.That(exception.InnerException, Is.TypeOf<AuthenticationException>());
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(1));
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(0));
        }

        [Test]
        public void DefaultPolicy_WillNotRetry_OnASelfSignedCertificate_RealButQuick()
        {
            //Arrange
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = new SocketsHttpHandler() });

            //Act
            async Task Act()
            {
                await httpClient
                    // this is a test website with a bunch of different certificate errors on it
                    .GetAsync("https://self-signed.badssl.com/")
                    .IfSuccessAsync(r => "".AsCompletedTask())
                    .ElseThrowAsync();
            }
            var exception = Assert.ThrowsAsync<HttpRequestException>(Act);

            //Assert
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(0));
            Assert.That(exception, Is.TypeOf<HttpRequestException>());
            Assert.That(exception.InnerException, Is.TypeOf<AuthenticationException>());
        }

        [Test]
        public async Task DefaultPolicy_WillRetry_OnConnectionTimeout()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequestAsync = (request, ctx, count) =>
                {
                    if (count == 0)
                    {
                        throw new TaskCanceledException("The operation was canceled.",
                                new TimeoutException("A connection could not be established within the configured ConnectTimeout."));
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK).AsCompletedTask();
                }
            };
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            var result = await httpClient
                    .PostAsJsonAsync("https://10.255.255.1/", "")
                    .IfSuccessAsync(r => true.AsCompletedTask())
                    .Else(r => false.AsCompletedTask());

            //Assert
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(2));
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().RetryReason, Is.EqualTo("ConnectTimeout"));
        }

        [Test]
        public void DefaultPolicy_WillRetry_OnConnectionTimeout_RealButQuick()
        {
            //Arrange
            // To make the retry work for a Connection Timeout have to use SocketsHttpHandler.  This is because:
            // 1) We want to set the connectionTimeout explicitly to 15 seconds (cos DNS should respond in this time)
            // 2) SocketsHttpHandler throws an exception which you can detect in Polly (a TaskCancelledException with an inner TimeoutException)
            // In the production code we use a Timeout of 15 seconds.
            // To make this test run fast we use a Timeout of 1 millisecond
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake) { RetryDelays = new() { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero } }.CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = new SocketsHttpHandler() { ConnectTimeout = TimeSpan.FromMilliseconds(1) } });

            //Act
            async Task Act()
            {
                // To get a connection timeout error, connect to a non-routable IP address such as 10.255.255.1
                // see https://stackoverflow.com/questions/100841/artificially-create-a-connection-timeout-error
                bool result = await httpClient
                    .PostAsJsonAsync("https://10.255.255.1/", "")
                    .IfSuccessAsync(r => true.AsCompletedTask())
                    .Else(r => false.AsCompletedTask());
            }
            var exception = Assert.ThrowsAsync<TaskCanceledException>(Act);

            //Assert
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(3));
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().RetryReason, Is.EqualTo("ConnectTimeout"));
            Assert.That(exception, Is.TypeOf<TaskCanceledException>());
            Assert.That(exception.Message, Is.EqualTo("The operation was canceled."));
            Assert.That(exception.InnerException, Is.TypeOf<TimeoutException>());
            Assert.That(exception.InnerException.Message, Is.EqualTo("A connection could not be established within the configured ConnectTimeout."));
        }

        [Test]
        public async Task DefaultPolicy_WillNotRetry_AnRetryTimeout()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequestAsync = async (request, ctx, count) =>
                {
                    // Simulate a slow response from an http call
                    await Task.Delay(TimeSpan.FromSeconds(1), ctx);
                    // But eventually return success
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };
            // Make the RetryTimeout very short (it is not actually used in DefaultPolicy at all, and that is why you don't get a timeout)
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake) { RetryTimeout = TimeSpan.FromMilliseconds(1) }.CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            bool result = await httpClient
                .GetAsync("http://example.com")
                .IfSuccessAsync(r => true.AsCompletedTask())
                .ElseThrowAsync();

            //Assert
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(1));
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task DefaultPolicy_WillLogRetries_OfRequestSideErrors()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequestAsync = (request, ctx, count) =>
                {
                    if (count == 0)
                    {
                        // Simulate a socket exception
                        var socketException = new SocketException((int)SocketError.HostNotFound);
                        return Task.FromException<HttpResponseMessage>(new HttpRequestException($"{socketException.Message} ({request?.RequestUri?.Host}:{request?.RequestUri?.Port})", socketException));
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK).AsCompletedTask();
                }
            };

            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            bool result = await httpClient
                .GetAsync("http://example.com")
                .IfSuccessAsync(r => true.AsCompletedTask())
                .ElseThrowAsync();

            //Assert
            Assert.That(result, Is.True);
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(2));

            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(1));
            var infoEvent = httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>();
            Assert.That(infoEvent.RetryReason, Is.EqualTo("SocketException"));
            Assert.That(infoEvent.RequestMethod, Is.Empty);
            Assert.That(infoEvent.RequestUri, Is.Empty);
            Assert.That(infoEvent.ResponseReasonPhrase, Is.Empty);
            Assert.That(infoEvent.ResponseStatusCode, Is.EqualTo(0));
            Assert.That(infoEvent.ExceptionType, Is.EqualTo("HttpRequestException"));
            Assert.That(infoEvent.Exception, Is.Not.Null);
        }

        [Test]
        public async Task DefaultPolicy_WillLogRetries_OfResponseSideErrors()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequest = (request, ctx, count) =>
                {
                    if (count == 0)
                    {
                        return new HttpResponseMessage(HttpStatusCode.GatewayTimeout);
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };

            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake).CreateDefaultPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            bool result = await httpClient
                .GetAsync("http://example.com")
                .IfSuccessAsync(r => true.AsCompletedTask())
                .ElseThrowAsync();

            //Assert
            Assert.That(result, Is.True);
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(2));
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(1));
            var infoEvent = httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>();
            Assert.That(infoEvent.RequestMethod, Is.Empty);
            Assert.That(infoEvent.RequestUri, Is.Empty);
            Assert.That(infoEvent.ResponseReasonPhrase, Is.EqualTo("Gateway Timeout"));
            Assert.That(infoEvent.ResponseStatusCode, Is.EqualTo(504));
            Assert.That(infoEvent.RetryCount, Is.EqualTo(1));
            Assert.That(infoEvent.RetryReason, Is.EqualTo("ResponseStatusCode"));
            Assert.That(infoEvent.Exception, Is.Null);
        }

        [Test]
        public async Task GetPolicy_WillRetry_ARetryTimeout()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequestAsync = async (request, ctx, count) =>
                {
                    if (count == 0)
                    {
                        // First try is slow to respond
                        await Task.Delay(TimeSpan.FromSeconds(10), ctx);
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };

            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake) { RetryTimeout = TimeSpan.FromMilliseconds(1) }.CreateGetPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            bool result = await httpClient
                .GetAsync("http://example.com")
                .IfSuccessAsync(r => true.AsCompletedTask())
                .ElseThrowAsync();


            //Assert
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(2));
            Assert.That(result, Is.True);
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(1));
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().RetryReason, Is.EqualTo("RetryTimeout"));
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().ExceptionType, Is.EqualTo("TimeoutRejectedException"));
            Assert.That(httpPolicyLoggerFake.Events[0].CastTo<PollyEvent>().Exception, Is.Not.Null);
        }

        [Test]
        public void GetPolicy_WillThrowATimeoutRejectedException_IfTheRetryTimeoutKeepsTimingOut()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequestAsync = async (request, ctx, count) =>
                {
                    // all calls to the handler are slow
                    await Task.Delay(TimeSpan.FromSeconds(10), ctx);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };

            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake)
            {
                RetryDelays = new() { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero },
                RetryTimeout = TimeSpan.FromMilliseconds(1)
            }.CreateGetPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            async Task Act()
            {
                bool result = await httpClient
                    .GetAsync("http://example.com")
                    .IfSuccessAsync(r => true.AsCompletedTask())
                    .ElseThrowAsync();
            }
            var exception = Assert.ThrowsAsync<TimeoutRejectedException>(Act);

            //Assert
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(4));
        }

        [Test]
        public void GetPolicy_WillNotRetry_AnHttpClientTimeout()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequestAsync = async (request, ctx, count) =>
                {
                    // Simulate a slow response from an http call
                    await Task.Delay(TimeSpan.FromSeconds(10), ctx);
                    // But eventually return success
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };
            // Make the httpclient timeout short so it will timeout before any retry stuff happens
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake) { RetryTimeout = TimeSpan.FromSeconds(20) }.CreateGetPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler }) { Timeout = TimeSpan.FromMilliseconds(100) };

            //Act
            async Task Act()
            {
                bool result = await httpClient
                    .GetAsync("http://example.com")
                    .IfSuccessAsync(r => true.AsCompletedTask())
                    .ElseThrowAsync();
            }
            var exception = Assert.ThrowsAsync<TaskCanceledException>(Act);

            //Assert
            Assert.That(httpClientHandler.TryCount, Is.EqualTo(1));
            Assert.That(exception.Message, Is.EqualTo("The request was canceled due to the configured HttpClient.Timeout of 0.1 seconds elapsing."));
            Assert.That(exception, Is.TypeOf<TaskCanceledException>());
            Assert.That(exception.InnerException, Is.TypeOf<TimeoutException>());
            Assert.That(exception.InnerException.Message, Is.EqualTo("A task was canceled."));
            Assert.That(exception.InnerException.InnerException, Is.TypeOf<TaskCanceledException>());
            Assert.That(exception.InnerException.InnerException.InnerException, Is.Null);
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task HttpClientFluent_WillCatchA_TimeoutRejectedException_IfYouHandleTimeout()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequestAsync = async (request, ctx, count) =>
                {
                    // Simulate slow response from http call
                    await Task.Delay(TimeSpan.FromSeconds(10), ctx);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };

            // Make the retry timeout quick, so it retrys and eventually throws a TimeoutRejectedException
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake)
            {
                RetryDelays = new() { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero },
                RetryTimeout = TimeSpan.FromMilliseconds(1)
            }.CreateGetPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler });

            //Act
            bool result = await httpClient
                .GetAsync("http://example.com")
                .IfSuccessAsync(r => true.AsCompletedTask())
                .ElseIfTimeoutAsync(() => false.AsCompletedTask())  // Under Test
                .ElseThrowAsync();

            //Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task HttpClientFluent_WillCatchA_TaskCancelledException_IfYouHandleTimeout()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequestAsync = async (request, ctx, count) =>
                {
                    // Simulate a slow response from an http call
                    await Task.Delay(TimeSpan.FromSeconds(10), ctx);
                    // But eventually return success
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };
            // Make the httpclient timeout short so it will timeout before any retry stuff happens
            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var policy = new HttpPolicyFactory(httpPolicyLoggerFake) { RetryTimeout = TimeSpan.FromSeconds(20) }.CreateGetPolicy();
            var httpClient = new HttpClient(new PolicyHttpMessageHandler(policy) { InnerHandler = httpClientHandler }) { Timeout = TimeSpan.FromMilliseconds(100) };

            //Act
            bool result = await httpClient
                .GetAsync("http://example.com")
                .IfSuccessAsync(r => true.AsCompletedTask())
                .ElseIfTimeoutAsync(() => false.AsCompletedTask())  // Under Test
                .ElseThrowAsync();

            //Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task Di_ForGetRequest_DiWillSelectTheGetPolicy_WhichRetriesOnTimeout()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequestAsync = async (request, ctx, count) =>
                {
                    if (count == 0)
                    {
                        // First try is slow to respond, so a retry will happen
                        await Task.Delay(TimeSpan.FromSeconds(1), ctx);
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };

            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var services = new ServiceCollection();
            services.AddPolicyRegistry(retryTimeout: TimeSpan.FromMilliseconds(100));
            services.AddHttpClientWithPolly("testclient", () => httpClientHandler);
            services.AddTransient<IHttpPolicyLogger>((u) => httpPolicyLoggerFake);
            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>();

            bool result = await httpClient.CreateClient("testclient")
                .GetAsync("http://example.com")
                .IfSuccessAsync(r => true.AsCompletedTask())
                .ElseThrowAsync();

            //Assert
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(1), httpPolicyLoggerFake.Events.ToJsonWithNoTypeNameHandlingIndented());
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task Di_ForPostRequests_DiWillSelectTheDefaultPolicy_WhichDoesNotRetryOnTimeout()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequestAsync = async (request, ctx, count) =>
                {
                    if (count == 0)
                    {
                        // First try is slow to respond, but a retry will not happen here
                        await Task.Delay(TimeSpan.FromSeconds(1), ctx);
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };


            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var services = new ServiceCollection();
            services.AddPolicyRegistry(retryTimeout: TimeSpan.FromMilliseconds(100));
            services.AddHttpClientWithPolly("testclient", () => httpClientHandler);
            services.AddTransient<IHttpPolicyLogger>((u) => httpPolicyLoggerFake);
            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>();

            // Act
            bool result = await httpClient.CreateClient("testclient")
                .PostAsJsonAsync("http://example.com", "")
                .IfSuccessAsync(r => true.AsCompletedTask())
                .ElseThrowAsync();

            //Assert
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(0));
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task Di_ForPostRequest_DiWillSelectTheGetPolicy_IfYouCallTheMethod_WithGetPolicy_WhenConfiguringTheHttpCall()
        {
            //Arrange
            var httpClientHandler = new CountingMessageHandler
            {
                HandleRequestAsync = async (request, ctx, count) =>
                {
                    if (count == 0)
                    {
                        // First try is slow to respond, so a retry will happen
                        await Task.Delay(TimeSpan.FromSeconds(1), ctx);
                    }
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            };

            var httpPolicyLoggerFake = new HttpPolicyLoggerFake();
            var services = new ServiceCollection();
            services.AddPolicyRegistry(retryTimeout: TimeSpan.FromMilliseconds(100));
            services.AddHttpClientWithPolly("testclient", () => httpClientHandler);
            services.AddTransient<IHttpPolicyLogger>((u) => httpPolicyLoggerFake);
            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>();

            // Act
            bool result = await httpClient.CreateClient("testclient")
                .WithGetPolicy() // <-- under test
                .PostJsonAsync("http://example.com", "")
                .IfSuccessAsync(r => true.AsCompletedTask())
                .ElseThrowAsync();

            //Assert
            Assert.That(httpPolicyLoggerFake.Events.Count, Is.EqualTo(1));
            Assert.That(result, Is.True);
        }
    }
}
