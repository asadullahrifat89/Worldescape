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
        public async Task<RepositoryResponse<long>> GetConstructsCount(string token, int worldId)
        {
            // Get constructs count for this world
            var response = await _httpServiceHelper.SendGetRequest<RecordsCountResponse>(
                actionUri: Constants.Action_GetConstructsCount,
                payload: new GetConstructsCountQueryRequest() { Token = token, WorldId = worldId });

            return RepositoryResponse<long>.BuildResponse(
                success: RepositoryResponse<long>.IsSuccess(response),
                result: response.Count,
                error: response.ExternalError);
        }

        /// <summary>
        /// Get constructs from server by pagination for the current world.
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<RepositoryResponse<Construct[]>> GetConstructs(string token, int pageIndex, int pageSize, int worldId)
        {
            // Get constructs in small packets
            var response = await _httpServiceHelper.SendGetRequest<RecordsResponse<Construct>>(
                actionUri: Constants.Action_GetConstructs,
                payload: new GetConstructsQueryRequest() { Token = token, PageIndex = pageIndex, PageSize = pageSize, WorldId = worldId });

            return RepositoryResponse<Construct[]>.BuildResponse(
                   success: RepositoryResponse<Construct[]>.IsSuccess(response),
                   result: response.Records,
                   error: response.ExternalError);
        }
    }
}
