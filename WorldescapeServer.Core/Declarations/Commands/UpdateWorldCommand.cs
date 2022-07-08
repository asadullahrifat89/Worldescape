namespace WorldescapeServer.Core;

/// <summary>
/// A command that updates a world.
/// </summary>
public class UpdateWorldCommand : AddWorldCommand
{
    /// <summary>
    /// The id of the world.
    /// </summary>
    public int Id { get; set; }
}

