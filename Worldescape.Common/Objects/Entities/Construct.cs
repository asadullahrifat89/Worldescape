namespace Worldescape.Common
{
    /// <summary>
    /// An object in a world.
    /// </summary>
    public class Construct : EntityBase
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
        /// The coordinate of this construct in the world.
        /// </summary>
        public Coordinate Coordinate { get; set; } = new Coordinate();

        /// <summary>
        /// The user who created this construct in a world.
        /// </summary>
        public Creator Creator { get; set; } = new Creator();
    }
}



