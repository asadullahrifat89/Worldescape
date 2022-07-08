using MediatR;
using Worldescape.Common;

namespace WorldescapeServer.Core;

/// <summary>
/// A query that fetches worlds.
/// </summary>
public class GetWorldsCountQuery : GetWorldsCountQueryRequest, IRequest<RecordsCountResponse>
{
    
}

