using System;

namespace Worldescape.Common
{
    /// <summary>
    /// Represents a chat message.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// The user's id which generated this message.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Used for text messages.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Used for image messages. DataUrl is passed here.
        /// </summary>
        public string Picture { get; set; }
    }

    public enum MessageType
    {
        Broadcast,
        Unicast
    }
}
