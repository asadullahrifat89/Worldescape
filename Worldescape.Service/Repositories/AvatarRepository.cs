using System.Threading.Tasks;
using Worldescape.Common;

namespace Worldescape.Service
{
    public class AvatarRepository
    {
        readonly HttpServiceHelper _httpServiceHelper;

        public AvatarRepository(HttpServiceHelper httpServiceHelper)
        {
            _httpServiceHelper = httpServiceHelper;
        }

        /// <summary>
        /// Get avatar count from server for the current world.
        /// </summary>
        /// <returns></returns>
        public async Task<RepositoryResponse> GetAvatarsCount(string token, int worldId)
        {
            // Get Avatars count for this world
            var response = await _httpServiceHelper.SendGetRequest<RecordsCountResponse>(
                actionUri: Constants.Action_GetAvatarsCount,
                payload: new GetAvatarsCountQueryRequest() { Token = token, WorldId = worldId });

            return RepositoryResponse.BuildResponse(
                success: RepositoryResponse.IsSuccess(response),
                result: response.Count,
                error: response.ExternalError);
        }

        /// <summary>
        /// Get avatars from server by pagination for the current world.
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<RepositoryResponse> GetAvatars(string token, int worldId, int pageSize, int pageIndex)
        {
            // Get Avatars in small packets
            var response = await _httpServiceHelper.SendGetRequest<RecordsResponse<Avatar>>(
                actionUri: Constants.Action_GetAvatars,
                payload: new GetAvatarsQueryRequest() { Token = token, PageIndex = pageIndex, PageSize = pageSize, WorldId = worldId });
                       
            return RepositoryResponse.BuildResponse(
                success: RepositoryResponse.IsSuccess(response),
                result: response.Records,
                error: response.ExternalError);
        }
    }
}
