using System.Net.Http;
using System.Threading.Tasks;
using Worldescape.Service;

namespace Worldescape
{
    public class HttpCommunicationService
    {
        private readonly IHttpService _httpService;
        public HttpCommunicationService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public string GetWebServiceUrl()
        {
#if DEBUG
            return Properties.Resources.DevWebService;
            //return Properties.Resources.ProdWebService;
#else
            return Properties.Resources.ProdWebService;
#endif
        }

        public async Task<HttpResponseMessage> SendToHttpAsync(HttpRequestMessage httpRequestMessage)
        {
            return await _httpService.SendAsync(httpRequestMessage);
        }

        public async Task<Response> SendToHttpAsync<Response>(
            HttpMethod httpMethod,
            string baseUri,
            string actionUri,
            object payload,
            string accessToken = null) where Response : class
        {
            var result = await _httpService.SendAsync<Response>(
               httpMethod: httpMethod,
               baseUri: baseUri,
               actionUri: actionUri,
               payload: payload,
               accessToken: accessToken);

            return result;
        }
    }
}
