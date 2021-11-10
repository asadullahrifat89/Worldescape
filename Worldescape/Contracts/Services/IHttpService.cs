using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Worldescape
{
    internal interface IHttpService
    {
		Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage);

		Task<Response> SendAsync<Response>(
			HttpMethod httpMethod,
			string baseUri,
			string actionUri,
			object payload = null,
			string accessToken = null) where Response : class;

		Task<Response> SendAsync<Response>(HttpRequestMessage httpRequestMessage) where Response : class;
	}
}
