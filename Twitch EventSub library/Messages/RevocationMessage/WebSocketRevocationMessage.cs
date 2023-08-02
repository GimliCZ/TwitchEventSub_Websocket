using Newtonsoft.Json;
using Twitch_EventSub_library.Messages.SharedContents;

namespace Twitch_EventSub_library.Messages.RevocationMessage
{
    public class WebSocketRevocationMessage : WebSocketMessage
    {
        [JsonProperty("payload")]
        public WebSocketSubscription? Payload { get; set; }
    }
}
