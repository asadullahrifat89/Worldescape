using MediatR;
using Worldescape.Data;

namespace WorldescapeWebService.Core;

/// <summary>
/// A command that inserts or updates a user.
/// </summary>
public class SaveBlobCommand : SaveBlobCommandRequest, IRequest<SaveBlobCommandResponse>
{

}

