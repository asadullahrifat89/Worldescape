using System.Net;

namespace Worldescape.Common
{
    public class RecordResponse<TRecord> : ServiceResponse
    {
        public TRecord Record { get; set; }

        public RecordResponse<TRecord> BuildSuccessResponse(TRecord record)
        {
            return new RecordResponse<TRecord>()
            {                
                Record = record,
                HttpStatusCode = HttpStatusCode.OK,
            };
        }

        public RecordResponse<TRecord> BuildErrorResponse(string error)
        {
            return new RecordResponse<TRecord>()
            {
                ExternalError = error,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }
    }
}