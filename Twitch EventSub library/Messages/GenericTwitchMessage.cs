using Newtonsoft.Json;
using Twitch.EventSub.Messages.SharedContents;

namespace Twitch.EventSub.Messages
{
    public abstract class WebSocketMessage
    {
        [JsonProperty("metadata")]
        public WebSocketMessageMetadata Metadata { get; set; }
    }
}