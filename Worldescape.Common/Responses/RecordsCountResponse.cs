using System.Net;

namespace Worldescape.Common
{
    public class RecordsCountResponse : ServiceResponse
    {
        /// <summary>
        /// Count of total Constructs returned.
        /// </summary>
        public long Count { get; set; }

        public RecordsCountResponse BuildSuccessResponse(long count)
        {
            return new RecordsCountResponse()
            {
                Count = count,
                HttpStatusCode = HttpStatusCode.OK,
            };
        }

        public RecordsCountResponse BuildErrorResponse(string error)
        {
            return new RecordsCountResponse()
            {
                ExternalError = error,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }
    }
}