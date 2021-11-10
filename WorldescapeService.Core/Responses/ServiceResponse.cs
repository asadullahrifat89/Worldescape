using System.Net;

namespace WorldescapeService.Core;

public class ServiceResponse
{
    public string RequestUri { get; set; }
    public string ExternalError { get; set; }
    public HttpStatusCode HttpStatusCode { get; set; }
}

