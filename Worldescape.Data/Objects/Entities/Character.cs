using System;

namespace Worldescape.Data
{
    /// <summary>
    /// A character for users to choose from when joining a world.
    /// </summary>
    public class Character : EntityBase
    {
        /// <summary>
        /// A list of image urls for each activity status of this character.
        /// </summary>
        public StatusBoundImageUrl[] StatusBoundImageUrls { get; set; } = Array.Empty<StatusBoundImageUrl>();
    }
}



