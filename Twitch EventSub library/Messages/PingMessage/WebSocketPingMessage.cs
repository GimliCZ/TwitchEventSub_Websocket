using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.PingMessage
{
    public class WebSocketPingMessage : WebSocketMessage
    {
        [JsonProperty("message_type")]
        public string MessageType { get; set; }
    }
}