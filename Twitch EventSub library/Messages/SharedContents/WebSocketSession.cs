using Newtonsoft.Json;

namespace Twitch_EventSub_library.Messages.SharedContents
{
    public class WebSocketSession
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("keepalive_timeout_seconds")]
        public int KeepAliveTimeoutSeconds { get; set; }

        [JsonProperty("reconnect_url")]
        public string? ReconnectUrl { get; set; }

        [JsonProperty("connected_at")]
        public string ConnectedAt { get; set; }
    }
}
