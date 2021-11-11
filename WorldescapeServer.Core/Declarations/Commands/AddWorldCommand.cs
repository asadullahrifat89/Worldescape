﻿using MediatR;
using Worldescape.Core;

namespace WorldescapeServer.Core;

/// <summary>
/// A command that inserts or updates a user.
/// </summary>
public class AddWorldCommand : RequestBase<World>
{
    /// <summary>
    /// Name of the world.
    /// </summary>
    public string Name { get; set; } = string.Empty;   

    /// <summary>
    /// Image url of the world.
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
}
