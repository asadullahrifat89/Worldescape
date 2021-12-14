using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Worldescape.Common
{
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

    public class RecordsResponse<TRecord> : ServiceResponse
    {
        public long Count { get; set; }

        public IEnumerable<TRecord> Records { get; set; } = Enumerable.Empty<TRecord>();

        public RecordsResponse<TRecord> BuildSuccessResponse(long count, IEnumerable<TRecord> records)
        {
            return new RecordsResponse<TRecord>()
            {
                Count = count,
                Records = records ?? Enumerable.Empty<TRecord>(),
                HttpStatusCode = HttpStatusCode.OK,
            };
        }

        public RecordsResponse<TRecord> BuildErrorResponse(string error)
        {
            return new RecordsResponse<TRecord>()
            {
                ExternalError = error,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }
    }

    public class RecordCountResponse : ServiceResponse
    {
        /// <summary>
        /// Count of total Constructs returned.
        /// </summary>
        public long Count { get; set; }

        public RecordCountResponse BuildSuccessResponse(long count)
        {
            return new RecordCountResponse()
            {
                Count = count,
                HttpStatusCode = HttpStatusCode.OK,
            };
        }

        public RecordCountResponse BuildErrorResponse(string error)
        {
            return new RecordCountResponse()
            {
                ExternalError = error,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }
    }
}