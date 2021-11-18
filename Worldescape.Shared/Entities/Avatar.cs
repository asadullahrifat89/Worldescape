namespace Worldescape.Shared
{
    /// <summary>
    /// Represents a user as an avatar in the world.
    /// </summary>
    public class Avatar : CoreBase
    {
        /// <summary>
        /// The character of this avatar.
        /// </summary>
        public Character Character { get; set; } = new Character();

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

    public enum ActivityStatus
    {
        Online,

        Moving,

        Working,

        Eating,

        Sleeping,

        Toilet,

        Meeting,

        Texting,

        Away,

        Offline,

        Crafting,
    }
}



