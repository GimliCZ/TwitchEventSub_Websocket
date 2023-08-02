using Newtonsoft.Json;

namespace Twitch_EventSub_library.Messages.ReconnectMessage
{
    public class WebSocketReconnectMessage : WebSocketMessage
    {
        [JsonProperty("payload")]
        public WebSocketReconnectPayload? Payload { get; set; }
    }
}
