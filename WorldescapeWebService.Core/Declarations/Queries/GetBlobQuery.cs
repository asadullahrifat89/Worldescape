using MediatR;
using Worldescape.Common;

namespace WorldescapeWebService.Core;

/// <summary>
/// A command that inserts or updates a user.
/// </summary>
public class GetBlobQuery : GetBlobQueryRequest, IRequest<byte[]>
{

}