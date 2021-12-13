namespace Worldescape.Common
{
    /// <summary>
    /// A world consisting of constructs and avatars.
    /// </summary>
    public class World : EntityBase
    {
        /// <summary>
        /// The User who created this world.
        /// </summary>
        public Creator Creator { get; set; } = new Creator();

        public new bool IsEmpty()
        {
            return Name.IsNullOrBlank() && (Creator == null || Creator.Id <= 0);
        }

        /// <summary>
        /// This increases as users log in and decreases as users log out.
        /// </summary>
        public int PopulationCount { get; set; } = 0;
    }
}