﻿using System;

namespace Worldescape.Common
{
    public class ChatMessage
    {
        public string Message { get; set; }
        public string Author { get; set; }
        public DateTime Time { get; set; }
        public string Picture { get; set; }
        public bool IsOriginNative { get; set; }
        public string AuthorProfilePictureUrl { get; set; }
    }

    public enum MessageType
    {
        Broadcast,
        Unicast
    }
}
