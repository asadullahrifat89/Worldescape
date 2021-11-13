﻿using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Reflection;
using System.Net.Http.Headers;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Worldescape.Core.Contracts.Services;
using Worldescape.Shared.Responses;

namespace Worldescape.Core.Services
{
    internal class HttpService : IHttpService, IDisposable
    {
        #region Fields

        private const string DefaultContentType = "application/json";
        private readonly HttpClient httpClient;
        private readonly IServiceProvider serviceProvider;

        #endregion

        #region Constructor

        public HttpService(
            HttpClient httpClient,
            IServiceProvider serviceProvider)
        {
            this.httpClient = httpClient;
            this.serviceProvider = serviceProvider;
        }
        #endregion

        #region Methods

        #region Contract

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage)
        {
            return await httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead);
        }

        public async Task<Response> SendAsync<Response>(HttpRequestMessage httpRequestMessage) where Response : class
        {
            using (var httpResponseMessage = await SendAsync(httpRequestMessage))
            {
                return await ParseResponse<Response>(httpRequest: httpRequestMessage, httpResponseMessage: httpResponseMessage);
            }
        }

        public async Task<Response> SendAsync<Response>(
            HttpMethod httpMethod,
            string baseUri,
            string actionUri,
            object payload = null,
            string accessToken = null) where Response : class
        {
            try
            {
                HttpRequestMessage httpRequestMessage = PrepareHttpRequest(
                    httpMethod: httpMethod,
                    baseUri: baseUri,
                    actionUri: actionUri,
                    payload: payload);

                if (!string.IsNullOrWhiteSpace(accessToken) && !string.IsNullOrEmpty(accessToken))
                {
                    httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                }

                using (var httpResponseMessage = await SendAsync(httpRequestMessage))
                {
                    var result = await ParseResponse<Response>(httpRequest: httpRequestMessage, httpResponseMessage: httpResponseMessage);

                    return result;
                }
            }
            catch (Exception e)
            {
                var requestUri = BuildRequestUri(
                    baseUri: baseUri,
                    actionUri: actionUri,
                    queryString: string.Empty).AbsoluteUri;

                return ReformedResponse(
                    result: default(Response),
                    statusCode: HttpStatusCode.InternalServerError,
                    requestUri: requestUri,
                    exception: e);
            }
        }

        #endregion

        #region Common

        public void Dispose()
        {
            httpClient.Dispose();
        }

        private static HttpRequestMessage PrepareHttpRequest(
            HttpMethod httpMethod,
            string baseUri,
            string actionUri,
            object payload)
        {
            HttpRequestMessage httpRequest = CreateHttpRequestMessage(
                httpMethod: httpMethod,
                baseUri: baseUri,
                actionUri: actionUri,
                payload: payload);

            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(DefaultContentType));
            return httpRequest;
        }

        private async Task<Response> ParseResponse<Response>(
            HttpRequestMessage httpRequest,
            HttpResponseMessage httpResponseMessage) where Response : class
        {
            // failed
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return ReformedResponse(
                    result: default(Response),
                    statusCode: httpResponseMessage.StatusCode,
                    requestUri: httpRequest.RequestUri.AbsoluteUri);
            }

            var jsonString = await httpResponseMessage.Content.ReadAsStringAsync();

            // string response
            if (typeof(Response).Equals(typeof(StringResponse)))
            {
                var stringResponse = new StringResponse
                {
                    Response = jsonString
                };

                return ReformedResponse(
                    result: stringResponse as Response,
                    statusCode: httpResponseMessage.StatusCode,
                    requestUri: httpRequest.RequestUri.AbsoluteUri);
            }

            var result = !string.IsNullOrWhiteSpace(jsonString) && !string.IsNullOrEmpty(jsonString) ? (Response)JsonConvert.DeserializeObject(
                    value: jsonString,
                    type: typeof(Response),
                    settings: new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) : default;

            return ReformedResponse(
                result: result,
                statusCode: httpResponseMessage.StatusCode,
                requestUri: httpRequest.RequestUri.AbsoluteUri);
        }

        private Response ReformedResponse<Response>(
            Response result,
            HttpStatusCode statusCode,
            string requestUri,
            Exception exception = null)
        {
            if (result == null)
            {
                result = (Response)ActivatorUtilities.CreateInstance(serviceProvider, typeof(Response));
            }

            var resultType = result.GetType();

            if (resultType.GetProperty("RequestUri") is PropertyInfo reqUri)
            {
                reqUri.SetValue(result, requestUri);
            }

            if (resultType.GetProperty("HttpStatusCode") is PropertyInfo httpStatuCode)
            {
                httpStatuCode.SetValue(result, statusCode);
            }

            if (resultType.GetProperty("ExternalError") is PropertyInfo extErr && exception != null)
            {
                extErr.SetValue(result, $"Exception: {exception.Message} StackTrace: {exception.StackTrace} on http request {requestUri}");
            }

            return result;
        }

        private static HttpRequestMessage CreateHttpRequestMessage(
            HttpMethod httpMethod,
            string baseUri,
            string actionUri,
            object payload)
        {
            var queryString = string.Empty;
            Uri requestUri = null;

            if (httpMethod == HttpMethod.Get)
            {
                if (payload != null)
                {
                    var parameters = from p in payload.GetType().GetProperties()
                                     where p.GetValue(payload, null) != null
                                     select $"{p.Name}={WebUtility.UrlEncode(p.GetValue(payload, null).ToString())}";

                    queryString = string.Join("&", parameters);

                    requestUri = BuildRequestUri(
                        baseUri: baseUri,
                        actionUri: actionUri,
                        queryString: queryString);
                }
                else
                {
                    requestUri = new Uri($"{baseUri}/{actionUri}");
                }
            }
            else
            {
                requestUri = BuildRequestUri(
                    baseUri: baseUri,
                    actionUri: actionUri,
                    queryString: queryString);
            }

            return new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = requestUri,
                Content = httpMethod != HttpMethod.Get && payload != null
                ? new StringContent(JsonConvert.SerializeObject(
                    value: payload,
                    formatting: Formatting.Indented,
                    settings: new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Include,
                    }), Encoding.UTF8, DefaultContentType)
                : null
            };
        }

        private static Uri BuildRequestUri(
            string baseUri,
            string actionUri,
            string queryString)
        {
            return new UriBuilder(baseUri)
            {
                Path = actionUri,
                Query = queryString,
            }.Uri;
        }
        #endregion

        #endregion
    }
}


