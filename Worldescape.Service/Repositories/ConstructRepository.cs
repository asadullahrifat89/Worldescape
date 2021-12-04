using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Worldescape.Common;

namespace Worldescape.Service
{
    public class ConstructRepository
    {
        readonly HttpServiceHelper _httpServiceHelper;

        public ConstructRepository(HttpServiceHelper httpServiceHelper)
        {
            _httpServiceHelper = httpServiceHelper;
        }

        /// <summary>
        ///  Get constructs count from server for the current world.
        /// </summary>
        /// <returns></returns>
        public async Task<RepositoryResponse> GetConstructsCount(string token, int worldId)
        {
            // Get constructs count for this world
            var response = await _httpServiceHelper.SendGetRequest<GetConstructsCountQueryResponse>(
                actionUri: Constants.Action_GetConstructsCount,
                payload: new GetConstructsCountQueryRequest() { Token = token, WorldId = worldId });

            return RepositoryResponse.BuildResponse(
                success: response.HttpStatusCode == System.Net.HttpStatusCode.OK && response.ExternalError.IsNullOrBlank(),
                result: response.Count,
                error: response.ExternalError);
        }

        /// <summary>
        /// Get constructs from server by pagination for the current world.
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<RepositoryResponse> GetConstructs(string token, int pageIndex, int pageSize, int worldId)
        {
            // Get constructs in small packets
            var response = await _httpServiceHelper.SendGetRequest<GetConstructsQueryResponse>(
                actionUri: Constants.Action_GetConstructs,
                payload: new GetConstructsQueryRequest() { Token = token, PageIndex = pageIndex, PageSize = pageSize, WorldId = worldId });

            return RepositoryResponse.BuildResponse(
                   success: response.HttpStatusCode == System.Net.HttpStatusCode.OK && response.ExternalError.IsNullOrBlank(),
                   result: response.Constructs,
                   error: response.ExternalError);
        }
    }
}
