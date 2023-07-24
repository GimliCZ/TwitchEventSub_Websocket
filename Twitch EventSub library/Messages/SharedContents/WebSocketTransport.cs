using Newtonsoft.Json;

namespace Twitch_EventSub_library.Messages.SharedContents
{
    public class WebSocketTransport
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("connected_at")]
        public string connectedAt { get; set; }
    }
}
