using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Worldescape.Core;

public class ServiceResponse
{
    public string RequestUri { get; set; } = string.Empty;
    public string ExternalError { get; set; } = string.Empty;
    public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.OK;
}

public class StringResponse : ServiceResponse
{
    public string Response { get; set; } = string.Empty;
}

