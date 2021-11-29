using System;

namespace Worldescape.Common
{
    /// <summary>
    /// Session information of a user in a world. This is used for connecting to Hub.
    /// </summary>
    public class AvatarSession
    {
        /// <summary>
        /// The connection id of the user.
        /// </summary>
        public string ConnectionId { get; set; } = string.Empty;

        public DateTime ReconnectionTime { get; set; }

        public DateTime DisconnectionTime { get; set; } = DateTime.MinValue;
    }
}