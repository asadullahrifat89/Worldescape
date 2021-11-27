namespace Worldescape.Data
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
        public AvatarSession Session { get; set; } = new AvatarSession();

        /// <summary>
        /// The activity status of the avatar in the world.
        /// </summary>
        public ActivityStatus ActivityStatus { get; set; }

        /// <summary>
        /// The coordinate of the avatar in the world.
        /// </summary>
        public Coordinate Coordinate { get; set; } = new Coordinate();
    }

    /// <summary>
    /// Activity statuses for avatar
    /// </summary>
    public enum ActivityStatus
    {
        //0
        Idle,

        //1
        Moving,

        //2
        Working,

        //3
        Inspecting,

        //4
        Resting,

        //5
        Greeting,

        //6
        Meeting,

        //7
        Messaging,

        //8
        Away,

        //9
        Offline,

        //10
        Crafting,
    }
}



