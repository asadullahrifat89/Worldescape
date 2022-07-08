using MediatR;
using Worldescape.Common;

namespace WorldescapeServer.Core;

public class GetUserQuery : GetUserQueryRequest, IRequest<RecordResponse<User>>
{
    
}

