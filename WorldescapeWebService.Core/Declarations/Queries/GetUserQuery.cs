using MediatR;
using Worldescape.Common;

namespace WorldescapeWebService.Core;

public class GetUserQuery : GetUserQueryRequest, IRequest<RecordResponse<User>>
{
    
}

