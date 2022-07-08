using MediatR;
using Worldescape.Common;

namespace WorldescapeServer.Core;

/// <summary>
/// A query that fetches Constructs.
/// </summary>
public class GetConstructsQuery : GetConstructsQueryRequest, IRequest<RecordsResponse<Construct>>
{
    
}

