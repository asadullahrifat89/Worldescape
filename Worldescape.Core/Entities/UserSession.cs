namespace Worldescape.Core
{
    /// <summary>
    /// A session for a user.
    /// </summary>
    public class UserSession
    {
        public DateTime DisconnectionTime { get; set; }

        public DateTime ReconnectionTime { get; set; }
    }
}
