using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.ReconnectMessage
{
    public class WebSocketReconnectMessage : WebSocketMessage
    {
        [JsonProperty("payload")]
        public WebSocketReconnectPayload? Payload { get; set; }
    }
}