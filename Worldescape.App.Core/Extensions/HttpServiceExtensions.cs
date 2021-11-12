using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System;
using System.Net;
using Worldescape.App.Core.Contracts.Services;
using Worldescape.App.Core.Services;

namespace Worldescape.App.Core.Extensions
{
    public static class HttpServiceExtensions
    {
        public static IServiceCollection AddHttpService(
            this IServiceCollection serviceCollection,
            int lifeTime = 300,
            int retryCount = 5,
            int retryWait = 2)
        {
            var policy = HttpPolicyExtensions
              .HandleTransientHttpError() // Handles HttpRequestException, Http status codes >= 500 (server errors) and status code 408 (request timeout)
              .Or<TimeoutRejectedException>()
              .OrResult(response => !response.IsSuccessStatusCode) // Retries if response status code does not indicate success
              .WaitAndRetryAsync(retryCount, _ => TimeSpan.FromSeconds(retryWait));

            serviceCollection.AddHttpClient<IHttpService, HttpService>()
               .SetHandlerLifetime(TimeSpan.FromSeconds(lifeTime))
               .AddPolicyHandler(policy);

            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) =>
            {
                return true;
                //return errors == SslPolicyErrors.None;
            };

            return serviceCollection;
        }
    }
}



