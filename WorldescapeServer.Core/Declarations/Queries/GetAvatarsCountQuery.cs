using MediatR;
using Worldescape.Common;

namespace WorldescapeServer.Core;

/// <summary>
/// A query that fetches Avatars.
/// </summary>
public class GetAvatarsCountQuery : GetAvatarsCountQueryRequest, IRequest<RecordsCountResponse>
{
    
}

