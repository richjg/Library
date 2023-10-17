using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using Polly;
using Polly.Timeout;

namespace Library
{
    public interface IHttpPolicyLogger
    {
        Task SendAsync(object customEvent);
    }

    public class HttpPolicyFactory
    {
        private readonly Random jitterer = new();
        private readonly IHttpPolicyLogger httpPolicyLogger;

        public List<TimeSpan> RetryDelays { get; set; } = new() { TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(4) };
        public TimeSpan RetryTimeout { get; set; } = TimeSpan.FromSeconds(20);

        public HttpPolicyFactory(IHttpPolicyLogger httpPolicyLogger)
        {
            this.httpPolicyLogger = httpPolicyLogger;
        }

        public IAsyncPolicy<HttpResponseMessage> CreateDefaultPolicy()
        {
            return CreateTooManyRequestsPolicy().WrapAsync(CreateBasePolicy());
        }

        public IAsyncPolicy<HttpResponseMessage> CreateGetPolicy()
        {
            return CreateDefaultPolicy().WrapAsync(Policy.TimeoutAsync(RetryTimeout));
        }

        private IAsyncPolicy<HttpResponseMessage> CreateBasePolicy()
        {
            // Polly docs               https://github.com/App-vNext/Polly
            // Socket exceptions        https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socketexception?view=net-5.0
            // Socket error codes       https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socketerror?view=net-5.0
            // Authentication exception https://docs.microsoft.com/en-us/dotnet/api/system.security.authentication.authenticationexception?view=net-5.0
            // Connection Timeout       https://github.com/dotnet/runtime/issues/66297
            // Polly Timeout            https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory#use-case-applying-timeouts
            //                          https://stackoverflow.com/questions/61441292/retry-multiple-times-when-http-request-times-out-using-polly-c-sharp
            // Retry 429 error          https://codereview.stackexchange.com/a/281417
            //
            // Timeouts:
            // HttpClient timeout (100 seconds)  - this is the overall timeout for the entire request process (including retries).
            //                                     if the timeout happens a TaskCancelledException is thrown
            //                                     this timeout is not (cannot) be retried
            // Connection timeout (15 seconds)   - this is the timeout for the socket connection to be established
            //                                     if this timeout happens a TaskCanceledException with an inner TimeoutException is thrown
            //                                     this timeout is retried for ALL requests
            // Retry timeout (20 seconds)        - this is the timeout for completion of a "try" of an http request.
            //                                     it only has an effect in the GetPolicy  (DefaultPolicy does not use it)
            //                                     if this timeout happens a TimeoutRejectedException is thrown
            //                                     this timeout is retried for GET request and selected POST requests
            //
            // Changed polly so will now retry all Socket exceptions (again).
            // The code before had a section in it, which stopped retry on certain socket exceptions.
            // I put this in because I thought that if you got a HostNotFound exception for example, retry would never work because the site simply does not exist.
            // However I now think that these errors can happen in a transitory way, because of DNS glitches, and so are worth retrying.

            // Changed so will retry on a connection timeout exception.
            // To make this change work had to change DI so that HttpClient is made with a SocketsHttpHandler with ConnectTimeout=15 seconds.
            // This means if a connection cannot be established in 15 seconds, an exception will be thrown and polly will retry.
            // It is completely safe to retry this case, as nothing has actually happened yet.
            // It does not change the overall timeout on HttpClient of 100 seconds, which means that if a connection does get established, it still has 100 seconds to complete.
            //
            // Changed so will retry a GET request after a timeout of 20 seconds.
            // If all the GET requests timeout, a TimeoutRejectedException will be thrown.
            // PUT/POST/PATCH will not retry, and will timeout with a TaskCancelled exception after 100 seconds.
            // see AddHttpClientWithPolly for where the policy is selected based on the request.
            //
            // Tweak so that on a 429 (Too Many Request) polly will use the Retry-After header value to set the time delay (if it is less than 20 seconds)
            // What we were seeing in the logs was a lot of double retries on Qna, where the first retry after 0.1 seconds always failed but the second retry after 1 second always worked (and qna apparently has a retry after of 1 second)
            // (Also in #5140 it failed out with too many requests, so improving the behaviour seems like a good idea to fix that problem)
            // (Also in #5132 found the retry delay for chatGPT was 20 seconds, so decided to set that as the upper limit for how long to wait)
            return Policy<HttpResponseMessage>
                                .Handle<Exception>(exception =>
                                {
                                    return exception switch
                                    {
                                        // Handles connection timeout exception (i.e. a failure for the Socket to establish a connection - probably due to a DNS glitch)
                                        // Note that for this to work the HttpClient must be configured to use a SocketsHttpHandler (see DI for where this is set up)
                                        TaskCanceledException when exception.InnerException is TimeoutException => true,
                                        HttpRequestException httpRequestException => httpRequestException.InnerException switch
                                        {
                                            // Any kind of ssl error should not retry
                                            AuthenticationException authenticationException => false,
                                            // Any kind of socket exception should retry
                                            SocketException socketException => true,
                                            // Any other exception retry (don't know any cases for this though)
                                            _ => true
                                        },
                                        // When polly times out, this exception is thrown and should be retried
                                        TimeoutRejectedException => true,
                                        _ => false
                                    };
                                })
                                .OrResult(r => r.StatusCode == HttpStatusCode.RequestTimeout
                                               || r.StatusCode == HttpStatusCode.InternalServerError
                                               || r.StatusCode == HttpStatusCode.BadGateway
                                               || r.StatusCode == HttpStatusCode.ServiceUnavailable
                                               || r.StatusCode == HttpStatusCode.GatewayTimeout)
                                .WaitAndRetryAsync(3,
                                    (count, response, context) =>
                                    {
                                        return RetryDelays[count - 1] + TimeSpan.FromMilliseconds(jitterer.Next(100));
                                    },
                                    (response, retryDelay, count, context) =>
                                    {
                                        string retryReason = response switch
                                        {
                                            _ when response?.Result?.StatusCode > 0 => "ResponseStatusCode",
                                            _ when response?.Exception.InnerException is SocketException => "SocketException",
                                            _ when response?.Exception is TimeoutRejectedException => "RetryTimeout",
                                            _ when response?.Exception is TaskCanceledException && response?.Exception.InnerException is TimeoutException => "ConnectTimeout",
                                            _ => "Unknown"
                                        };

                                        httpPolicyLogger.SendAsync(new PollyEvent()
                                        {
                                            RetryCount = count,
                                            RetryReason = retryReason,
                                            RetryDelay = retryDelay,
                                            RequestUri = response?.Result?.RequestMessage?.RequestUri?.ToString() ?? string.Empty,
                                            RequestMethod = response?.Result?.RequestMessage?.Method?.ToString() ?? string.Empty,
                                            ResponseStatusCode = (int?)response?.Result?.StatusCode ?? 0,
                                            ResponseReasonPhrase = response?.Result?.ReasonPhrase ?? string.Empty,
                                            ExceptionType = response?.Exception?.GetType().Name ?? string.Empty,
                                            Exception = response?.Exception,
                                        });

                                        return Task.CompletedTask;
                                    });
        }

        public IAsyncPolicy<HttpResponseMessage> CreateTooManyRequestsPolicy()
        {
            return Policy<HttpResponseMessage>
                .HandleResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(15,
                                    (count, response, context) =>
                                    {
                                        var delay = response.Result?.Headers.RetryAfter?.Delta;
                                        if (delay == null)
                                            return TimeSpan.FromSeconds(4);
                                        else if (delay <= TimeSpan.FromSeconds(20))
                                            return delay.Value;
                                        else
                                            return TimeSpan.FromSeconds(20);
                                    },
                                    (response, retryDelay, count, context) =>
                                    {
                                        httpPolicyLogger.SendAsync(new PollyEvent()
                                        {
                                            RetryCount = count,
                                            RetryReason = "ResponseStatusCode",
                                            RetryDelay = retryDelay,
                                            RequestUri = response?.Result?.RequestMessage?.RequestUri?.ToString() ?? string.Empty,
                                            RequestMethod = response?.Result?.RequestMessage?.Method?.ToString() ?? string.Empty,
                                            ResponseStatusCode = (int?)response?.Result?.StatusCode ?? 0,
                                            ResponseReasonPhrase = response?.Result?.ReasonPhrase ?? string.Empty,
                                            ExceptionType = response?.Exception?.GetType().Name ?? string.Empty,
                                            Exception = response?.Exception,
                                        });

                                        return Task.CompletedTask;
                                    });
        }
    }

    public class PollyEvent
    {
        public int RetryCount { get; set; }
        public string RetryReason { get; set; } = string.Empty;
        public TimeSpan RetryDelay { get; set; }
        public string RequestUri { get; set; } = string.Empty;
        public string RequestMethod { get; set; } = string.Empty;
        public int ResponseStatusCode { get; set; }
        public string ResponseReasonPhrase { get; set; } = string.Empty;
        public string ExceptionType { get; set; } = string.Empty;
        public object? Exception { get; set; }
    }
}
