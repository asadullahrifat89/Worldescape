using MediatR;
using Worldescape.Common;

namespace WorldescapeServer.Core;

/// <summary>
/// A command that inserts or updates a user.
/// </summary>
public class AddUserCommand : AddUserCommandRequest, IRequest<ServiceResponse>
{

}

