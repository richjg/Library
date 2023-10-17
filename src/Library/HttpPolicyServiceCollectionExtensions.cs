using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Library
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterHttpServices(this IServiceCollection services)
        {
            services.AddPolicyRegistry(TimeSpan.FromSeconds(20));
            //services.AddHttpClientNoPolly<IHttpClientProvider<HttpCheckConnectionInfo>, HttpClientProvider<HttpCheckConnectionInfo>>();
            //services.AddTransient<IHttpClientScopeFactory<HttpCheckConnectionInfo>, HttpClientScopeFactory<HttpCheckConnectionInfo>>();
            //services.AddTransient<IAmbientHttpClientLocator<HttpCheckConnectionInfo>, AmbientHttpClientLocator<HttpCheckConnectionInfo>>();
        }

        public static void AddPolicyRegistry(this IServiceCollection services, TimeSpan retryTimeout)
        {
            services.AddPolicyRegistry((sp, policyRegistry) =>
            {
                policyRegistry.Add("DefaultPolicy", new HttpPolicyFactory(sp.GetRequiredService<IHttpPolicyLogger>()) { RetryTimeout = retryTimeout }.CreateDefaultPolicy());
                policyRegistry.Add("GetPolicy", new HttpPolicyFactory(sp.GetRequiredService<IHttpPolicyLogger>()) { RetryTimeout = retryTimeout }.CreateGetPolicy());
            });
        }

        public static IHttpClientBuilder AddHttpClientWithPolly<TClient, TImplementation>(this IServiceCollection services, Func<HttpMessageHandler>? configurePrimaryHttpMessageHandler = null) where TClient : class where TImplementation : class, TClient
        {
            configurePrimaryHttpMessageHandler ??= () => new SocketsHttpHandler() { ConnectTimeout = TimeSpan.FromSeconds(15), UseCookies = false };
            return services.AddHttpClient<TClient, TImplementation>()
                .ConfigurePrimaryHttpMessageHandler(configurePrimaryHttpMessageHandler)
                .AddPolicyHandlerFromRegistry((policyRegistry, request) =>
                {
                    request.Headers.TryGetValues("http-policy", out var policyHeaders);
                    string policy = policyHeaders?.FirstOrDefault() switch
                    {
                        "get" => "GetPolicy",
                        _ => request.Method == HttpMethod.Get ? "GetPolicy" : "DefaultPolicy",
                    };
                    return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>(policy);
                });
        }

        public static IHttpClientBuilder AddHttpClientWithPolly(this IServiceCollection services, string name, Func<HttpMessageHandler>? configurePrimaryHttpMessageHandler = null)
        {
            configurePrimaryHttpMessageHandler ??= () => new SocketsHttpHandler() { ConnectTimeout = TimeSpan.FromSeconds(15), UseCookies = false };
            return services.AddHttpClient(name)
                .ConfigurePrimaryHttpMessageHandler(configurePrimaryHttpMessageHandler)
                .AddPolicyHandlerFromRegistry((policyRegistry, request) =>
                {
                    request.Headers.TryGetValues("http-policy", out var policyHeaders);
                    string policy = policyHeaders?.FirstOrDefault() switch
                    {
                        "get" => "GetPolicy",
                        _ => request.Method == HttpMethod.Get ? "GetPolicy" : "DefaultPolicy",
                    };
                    return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>(policy);
                });
        }

        public static IHttpClientBuilder AddHttpClientNoPolly<TClient, TImplementation>(this IServiceCollection services) where TClient : class where TImplementation : class, TClient
        {
            return services.AddHttpClient<TClient, TImplementation>()
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler() { ConnectTimeout = TimeSpan.FromSeconds(15), UseCookies = false });
        }
    }
}
