using MongoDB.Bson.Serialization.Attributes;

namespace Worldescape.Common
{
    /// <summary>
    /// Information of a user who created a construct or a world.
    /// </summary>
    public class Creator
    {
        /// <summary>
        /// User's id.
        /// </summary>
        [BsonId] 
        public int Id { get; set; }

        /// <summary>
        /// User's name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Image Url of the user.
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
    }
}