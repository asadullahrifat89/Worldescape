using System;
using System.Collections.Generic;
using System.Text;
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
            var response = await _httpServiceHelper.SendGetRequest<GetWorldsCountQueryResponse>(
                actionUri: Constants.Action_GetWorldsCount,
                payload: new GetWorldsCountQueryRequest() { Token = token, SearchString = searchString, CreatorId = creatorId });

            return RepositoryResponse.BuildResponse(
                success: response.HttpStatusCode == System.Net.HttpStatusCode.OK && response.ExternalError.IsNullOrBlank(),
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
            var response = await _httpServiceHelper.SendGetRequest<GetWorldsQueryResponse>(
                actionUri: Constants.Action_GetWorlds,
                payload: new GetWorldsQueryRequest() { Token = token, PageIndex = pageIndex, PageSize = pageSize, SearchString = searchString, CreatorId = creatorId });

            return RepositoryResponse.BuildResponse(
                   success: response.HttpStatusCode == System.Net.HttpStatusCode.OK && response.ExternalError.IsNullOrBlank(),
                   result: response.Worlds,
                   error: response.ExternalError);
        }

        public async Task<RepositoryResponse> AddWorld(string token, string name, string imageUrl)
        {
            var command = new AddWorldCommandRequest
            {
                Token = token,
                Name = name,
                ImageUrl = imageUrl
            };

            var world = await _httpServiceHelper.SendPostRequest<World>(
               actionUri: Constants.Action_AddWorld,
               payload: command);

            return RepositoryResponse.BuildResponse(
                success: world != null && world.Id > 0,
                result: world,
                error: world != null && world.Id > 0 ? null : "Failed to create your world. This shouldn't be happening. Try again.");
        }
    }
}
