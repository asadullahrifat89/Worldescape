namespace Worldescape.Core;

/// <summary>
/// An object in a world.
/// </summary>
public class Construct
{
    /// <summary>
    /// The world in which this construct is.
    /// </summary>
    public InWorld World { get; set; } = new InWorld();

    /// <summary>
    /// The applied rotation of this construct.
    /// </summary>
    public float Rotation { get; set; } = 0;

    /// <summary>
    /// The applied scale of this construct.
    /// </summary>
    public float Scale { get; set; } = 1;

    /// <summary>
    /// The coordinate to be used when cloning this construct.
    /// </summary>
    public Coordinate CloneCoordinate { get; set; } = new Coordinate();

    /// <summary>
    /// The center point coordinate of this construct.
    /// </summary>
    public Coordinate CenterPoint { get; set; } = new Coordinate();

    /// <summary>
    /// The user who created this construct in a world.
    /// </summary>
    public Creator Creator { get; set; } = new Creator();
}

