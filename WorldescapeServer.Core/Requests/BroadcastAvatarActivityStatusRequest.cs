﻿using static Worldescape.Core.Enums;

namespace WorldescapeServer.Core
{
    public class BroadcastAvatarActivityStatusRequest
    {
        /// <summary>
        /// Id of the avatar who's activity status changed.
        /// </summary>
        public string AvatarId { get; set; } = string.Empty;

        /// <summary>
        /// Activity status of the avatar.
        /// </summary>
        public ActivityStatus ActivityStatus { get; set; }
    }
}
