using MediatR;
using Worldescape.Shared.Responses;

namespace WorldescapeWebService.Core.Declarations.Queries;

/// <summary>
/// A command that inserts or updates a user.
/// </summary>
public class GetApiTokenQuery : IRequest<StringResponse>
{
    /// <summary>
    /// The email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The password of the user.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

