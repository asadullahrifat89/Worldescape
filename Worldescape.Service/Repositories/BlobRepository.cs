using System.Threading.Tasks;
using Worldescape.Common;

namespace Worldescape.Service
{
    public class BlobRepository
    {
        readonly HttpServiceHelper _httpServiceHelper;

        public BlobRepository(HttpServiceHelper httpServiceHelper)
        {
            _httpServiceHelper = httpServiceHelper;
        }

        /// <summary>
        /// Save a blod.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dataUrl"></param>
        /// <returns></returns>
        public async Task<RepositoryResponse> SaveBlob(string token, string dataUrl)
        {
            var command = new SaveBlobCommandRequest()
            {
                Id = UidGenerator.New(),
                DataUrl = dataUrl,
                Token = token
            };

            var response = await _httpServiceHelper.SendPostRequest<SaveBlobCommandResponse>(
              actionUri: Constants.Action_SaveBlob,
              payload: command);

            return RepositoryResponse.BuildResponse(
                   success: RepositoryResponse.IsSuccess(response),
                   result: response.Id,
                   error: response.ExternalError);
        }
    }
}
