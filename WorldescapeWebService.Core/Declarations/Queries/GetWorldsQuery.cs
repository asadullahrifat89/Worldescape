using MediatR;
using Worldescape.Data;

namespace WorldescapeWebService.Core;

/// <summary>
/// A query that fetches worlds.
/// </summary>
public class GetWorldsQuery : GetWorldsQueryRequest, IRequest<GetWorldsQueryResponse>
{
    
}

