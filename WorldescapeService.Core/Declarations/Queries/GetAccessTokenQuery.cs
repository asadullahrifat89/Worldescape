using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Worldescape.Core;

namespace WorldescapeService.Core;

/// <summary>
/// A command that inserts or updates a user.
/// </summary>
public class GetAccessTokenQuery : IRequest<AccessToken>
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

