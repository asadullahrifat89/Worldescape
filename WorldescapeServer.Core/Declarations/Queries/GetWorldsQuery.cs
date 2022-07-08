using MediatR;
using Worldescape.Common;

namespace WorldescapeServer.Core;

/// <summary>
/// A query that fetches worlds.
/// </summary>
public class GetWorldsQuery : GetWorldsQueryRequest, IRequest<RecordsResponse<World>>
{
    
}

