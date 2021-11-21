using MediatR;
using Worldescape.Data;

namespace WorldescapeWebService.Core;

public class GetUserQuery : GetUserQueryRequest, IRequest<User>
{
    
}

