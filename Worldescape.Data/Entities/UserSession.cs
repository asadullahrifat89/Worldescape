using System;

namespace Worldescape.Data
{
    /// <summary>
    /// Session information of a user in a world.
    /// </summary>
    public class UserSession
    {
        public DateTime ReconnectionTime { get; set; }

        public DateTime DisconnectionTime { get; set; } = DateTime.MinValue;
    }
}