using System.Net;
using Worldescape.Common;

namespace Worldescape.Service
{
    public class RepositoryResponse<T>
    {
        public T Result { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; } = string.Empty;

        public static RepositoryResponse<T> BuildResponse(bool success, T result, string error = null)
        {
            return new RepositoryResponse<T>
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
