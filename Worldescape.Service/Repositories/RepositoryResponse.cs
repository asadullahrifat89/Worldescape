using System.Net;
using Worldescape.Common;

namespace Worldescape.Service
{
    public class RepositoryResponse
    {
        public object Result { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;

        public static RepositoryResponse BuildResponse(bool success, object result, string error = null)
        {
            return new RepositoryResponse
            {
                Error = error,
                Success = success,
                Result = result
            };
        }

        public static bool IsSuccess(ServiceResponse response)
        {
            return response.HttpStatusCode == HttpStatusCode.OK && response.ExternalError.IsNullOrBlank();
        }
    }
}
