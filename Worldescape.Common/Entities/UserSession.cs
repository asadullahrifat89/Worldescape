namespace Worldescape.App.Core;

/// <summary>
/// Session information of a user in a world.
/// </summary>
public class UserSession
{
    public DateTime DisconnectionTime { get; set; } = DateTime.MinValue;

    public DateTime ReconnectionTime { get; set; }
}

