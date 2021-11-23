using MediatR;
using Worldescape.Data;

namespace WorldescapeWebService.Core;

/// <summary>
/// A query that fetches Constructs.
/// </summary>
public class GetConstructsCountQuery : GetConstructsCountQueryRequest, IRequest<GetConstructsCountQueryResponse>
{
    
}

