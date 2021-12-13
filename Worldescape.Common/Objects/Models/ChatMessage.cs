namespace Worldescape.Common
{
    /// <summary>
    /// Represents a chat message.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Id of the message. This is used to track replies.
        /// </summary>
        public int Id { get; set; } = UidGenerator.New();

        /// <summary>
        /// The sender avatar's id.
        /// </summary>
        public int SenderId { get; set; }

        /// <summary>
        /// The recipient avatar's id.
        /// </summary>
        public int RecipientId { get; set; }

        /// <summary>
        /// Used for text messages.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Used for image messages. DataUrl is passed here.
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        /// Id of the message to which this message is a reply.
        /// </summary>
        public int ReplyToMessageId { get; set; }
    }

    public enum MessageType
    {
        Broadcast,
        Unicast
    }
}
