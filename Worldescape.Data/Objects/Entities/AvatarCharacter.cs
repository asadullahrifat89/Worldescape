﻿using MongoDB.Bson.Serialization.Attributes;

namespace Worldescape.Data
{
    /// <summary>
    /// Information of an avatar's character.
    /// </summary>
    public class AvatarCharacter
    {
        /// <summary>
        /// Characte's id.
        /// </summary>
        [BsonId]
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


