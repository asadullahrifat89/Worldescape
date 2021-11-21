using MediatR;
using Worldescape.Data;

namespace WorldescapeWebService.Core;

/// <summary>
/// A command that inserts or updates a user.
/// </summary>
public class AddUserCommand : AddUserCommandRequest, IRequest<ServiceResponse>
{

}

