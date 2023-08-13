using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.SharedContents
{
    public class WebSocketTransport
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("connected_at")]
        public DateTime connectedAt { get; set; }

        [JsonProperty("disconnected_at")]
        public DateTime DisconnectedAt { get; set; }
    }
}
