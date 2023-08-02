using Newtonsoft.Json;

namespace Twitch_EventSub_library.Messages.PingMessage
{
    public class WebSocketPingMessage : WebSocketMessage
    {
        [JsonProperty("message_type")]
        public string MessageType { get; set; }
    }
}
