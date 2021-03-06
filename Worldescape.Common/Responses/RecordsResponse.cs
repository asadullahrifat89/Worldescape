using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Worldescape.Common
{
    public class RecordsResponse<TRecord> : ServiceResponse
    {
        public long Count { get; set; }

        public TRecord[] Records { get; set; } = new TRecord[] { };

        public RecordsResponse<TRecord> BuildSuccessResponse(long count, IEnumerable<TRecord> records)
        {
            return new RecordsResponse<TRecord>()
            {
                Count = count,
                Records = records != null ? records.ToArray() : new TRecord[] { },
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
}