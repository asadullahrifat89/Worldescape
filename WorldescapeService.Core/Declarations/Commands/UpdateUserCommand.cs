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
public class UpdateUserCommand : AddUserCommand
{
    /// <summary>
    /// The id of the user.
    /// </summary>
    public int Id { get; set; }
}

