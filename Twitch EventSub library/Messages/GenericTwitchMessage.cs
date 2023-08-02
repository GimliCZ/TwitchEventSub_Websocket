﻿using Newtonsoft.Json;
using Twitch_EventSub_library.Messages.SharedContents;

namespace Twitch_EventSub_library.Messages
{
    public abstract class WebSocketMessage
    {
        [JsonProperty("metadata")]
        public WebSocketMessageMetadata? Metadata { get; set; }
    }
}
