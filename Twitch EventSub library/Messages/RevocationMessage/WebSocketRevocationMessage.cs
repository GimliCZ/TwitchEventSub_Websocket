using Newtonsoft.Json;
using Twitch.EventSub.Messages.SharedContents;

namespace Twitch.EventSub.Messages.RevocationMessage
{
    public class WebSocketRevocationMessage : WebSocketMessage
    {
        [JsonProperty("payload")]
        public WebSocketSubscription? Payload { get; set; }
    }
}
