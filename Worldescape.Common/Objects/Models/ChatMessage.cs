using System;

namespace Worldescape.Common
{
    /// <summary>
    /// Represents a chat message.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Used for text messages.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Used for image messages. DataUrl is passed here.
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        /// Time when this message was generated.
        /// </summary>
        public DateTime Time { get; set; } = DateTime.Now;
    }

    public enum MessageType
    {
        Broadcast,
        Unicast
    }
}
