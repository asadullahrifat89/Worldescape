using System.Threading.Tasks;
using Worldescape.Common;

namespace Worldescape.Service
{
    public class WorldRepository
    {
        readonly HttpServiceHelper _httpServiceHelper;

        public WorldRepository(HttpServiceHelper httpServiceHelper)
        {
            _httpServiceHelper = httpServiceHelper;
        }

        /// <summary>
        ///  Get Worlds count from server for the current world.
        /// </summary>
        /// <returns></returns>
        public async Task<RepositoryResponse> GetWorldsCount(string token, string searchString, int creatorId)
        {
            // Get Worlds count for this world
            var response = await _httpServiceHelper.SendGetRequest<RecordsCountResponse>(
                actionUri: Constants.Action_GetWorldsCount,
                payload: new GetWorldsCountQueryRequest() { Token = token, SearchString = searchString, CreatorId = creatorId });

            return RepositoryResponse.BuildResponse(
                success: RepositoryResponse.IsSuccess(response),
                result: response.Count,
                error: response.ExternalError);
        }

        /// <summary>
        /// Get Worlds from server by pagination for the current world.
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<RepositoryResponse> GetWorlds(string token, int pageIndex, int pageSize, string searchString, int creatorId)
        {
            // Get Worlds in small packets
            var response = await _httpServiceHelper.SendGetRequest<RecordsResponse<World>>(
                actionUri: Constants.Action_GetWorlds,
                payload: new GetWorldsQueryRequest() { Token = token, PageIndex = pageIndex, PageSize = pageSize, SearchString = searchString, CreatorId = creatorId });

            return RepositoryResponse.BuildResponse(
                   success: RepositoryResponse.IsSuccess(response),
                   result: response.Records,
                   error: response.ExternalError);
        }

        /// <summary>
        /// Adds a world.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="name"></param>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
        public async Task<RepositoryResponse> AddWorld(string token, string name, string imageUrl)
        {
            var command = new AddWorldCommandRequest
            {
                Token = token,
                Name = name,
                ImageUrl = imageUrl
            };

            var response = await _httpServiceHelper.SendPostRequest<RecordResponse<World>>(
               actionUri: Constants.Action_AddWorld,
               payload: command);

            var success = RepositoryResponse.IsSuccess(response);

            return RepositoryResponse.BuildResponse(
                success: success,
                result: response.Record,
                error: success ? null : "Failed to create your world. This shouldn't be happening. Try again.");
        }

        /// <summary>
        /// Updates a world.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RepositoryResponse> UpdateWorld(string token, string name, int id)
        {
            var command = new UpdateWorldCommandRequest
            {
                Token = token,
                Name = name,
                Id = id
            };

            var response = await _httpServiceHelper.SendPostRequest<RecordResponse<World>>(
               actionUri: Constants.Action_UpdateWorld,
               payload: command);

            var success = RepositoryResponse.IsSuccess(response);

            return RepositoryResponse.BuildResponse(
                success: success,
                result: response.Record,
                error: success ? null : "Failed to save your world. This shouldn't be happening. Try again.");
        }
    }
}
