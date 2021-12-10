using System;
using System.Collections.Generic;
using System.Text;

namespace Worldescape.Common
{
    public class Portal
    {
        /// <summary>
        /// The coordinate of this portal in the world.
        /// </summary>
        public Coordinate Coordinate { get; set; } = new Coordinate();

        /// <summary>
        /// The world to which this portals takes.
        /// </summary>
        public World World { get; set; }
    }
}
