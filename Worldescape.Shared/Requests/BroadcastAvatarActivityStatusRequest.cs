﻿using Worldescape.Shared.Entities;

namespace Worldescape.Shared.Requests
{
    public class BroadcastAvatarActivityStatusRequest
    {
        /// <summary>
        /// Id of the avatar who's activity status changed.
        /// </summary>
        public int AvatarId { get; set; }

        /// <summary>
        /// Activity status of the avatar.
        /// </summary>
        public ActivityStatus ActivityStatus { get; set; }
    }
}