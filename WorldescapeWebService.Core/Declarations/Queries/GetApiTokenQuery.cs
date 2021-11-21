using MediatR;
using Worldescape.Data;

namespace WorldescapeWebService.Core;

/// <summary>
/// A command that inserts or updates a user.
/// </summary>
public class GetApiTokenQuery : GetApiTokenQueryRequest, IRequest<StringResponse>
{
    ///// <summary>
    ///// The email address of the user.
    ///// </summary>
    //public string Email { get; set; } = string.Empty;

    ///// <summary>
    ///// The password of the user.
    ///// </summary>
    //public string Password { get; set; } = string.Empty;
}