using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Worldescape.Core;

namespace WorldescapeServer.Core
{
    public class BroadcastAvatarMovementRequest
    {
        /// <summary>
        /// Id of the avatar who moved in a world.
        /// </summary>
        public int AvatarId { get; set; }

        /// <summary>
        /// Coordinate of the avatar's movement.
        /// </summary>
        public Coordinate Coordinate { get; set; } = new Coordinate();
    }
}
