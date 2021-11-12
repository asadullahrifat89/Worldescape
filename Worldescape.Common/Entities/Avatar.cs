using static Worldescape.App.Core.Enums;

namespace Worldescape.App.Core;

/// <summary>
/// An entity in a world representing a user with the user's character.
/// </summary>
public class Avatar : CoreBase
{
    /// <summary>
    /// The character of this avatar.
    /// </summary>
    public AvatarCharacter Character { get; set; } = new AvatarCharacter();

    /// <summary>
    /// The user which this avatar is representing.
    /// </summary>
    public AvatarUser User { get; set; } = new AvatarUser();

    /// <summary>
    /// The world in which this avatar is.
    /// </summary>
    public InWorld World { get; set; } = new InWorld();

    /// <summary>
    /// The user's session of this avatar in the world.
    /// </summary>
    public UserSession Session { get; set; } = new UserSession();

    /// <summary>
    /// The connection id of the avatar.
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// The activity status of the avatar in the world.
    /// </summary>
    public ActivityStatus ActivityStatus { get; set; }

    /// <summary>
    /// The coordinate of the avatar in the world.
    /// </summary>
    public Coordinate Coordinate { get; set; } = new Coordinate();
}

