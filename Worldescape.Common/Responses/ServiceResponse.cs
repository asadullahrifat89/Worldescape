using System.Net;

namespace Worldescape.Common
{
    public class ServiceResponse
    {
        public string RequestUri { get; set; } = string.Empty;
        public string ExternalError { get; set; } = string.Empty;
        public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.OK;
    }
}