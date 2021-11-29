using MediatR;
using Worldescape.Common;

namespace WorldescapeWebService.Core;

/// <summary>
/// A query that fetches Avatars.
/// </summary>
public class GetAvatarsQuery : GetAvatarsQueryRequest, IRequest<GetAvatarsQueryResponse>
{
    
}

