using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldescapeService.Core;

/// <summary>
/// A command that inserts or updates a user.
/// </summary>
public class AddUserCommand : IRequest<ServiceResponse>
{
    /// <summary>
    /// Name of the user.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The phone number of the user.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// The password of the user.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Image url of the user.
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
}

