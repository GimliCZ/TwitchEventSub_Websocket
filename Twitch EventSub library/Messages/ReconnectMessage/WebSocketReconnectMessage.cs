using Newtonsoft.Json;
using Twitch_EventSub_library.Messages.SharedContents;

namespace Twitch_EventSub_library.Messages.ReconnectMessage
{
    public class WebSocketReconnectMessage: WebSocketMessage
    {
        [JsonProperty("payload")]
        public WebSocketReconnectPayload Payload { get; set; }
    }
}
