namespace Worldescape.Common.Entities
{
    /// <summary>
    /// Information of an avatar's character.
    /// </summary>
    public class AvatarCharacter
    {
        /// <summary>
        /// Characte's id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Characte's name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Image Url of the character.
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
    }
}



