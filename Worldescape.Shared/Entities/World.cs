﻿namespace Worldescape.Shared.Entities
{
    /// <summary>
    /// A world consisting of constructs and avatars.
    /// </summary>
    public class World : CoreBase
    {
        /// <summary>
        /// The User who created this world.
        /// </summary>
        public Creator Creator { get; set; } = new Creator();
    }
}