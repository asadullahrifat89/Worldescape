namespace Worldescape.Core;

/// <summary>
/// Session information of a user in a world.
/// </summary>
public class UserSession
{
    public DateTime DisconnectionTime { get; set; }

    public DateTime ReconnectionTime { get; set; }
}

