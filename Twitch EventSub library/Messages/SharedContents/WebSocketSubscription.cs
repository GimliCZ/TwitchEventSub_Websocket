using Newtonsoft.Json;
using Twitch.EventSub.API.Models;

namespace Twitch.EventSub.Messages.SharedContents
{
    public class WebSocketSubscription
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("cost")]
        public string Cost { get; set; }

        [JsonProperty("condition")]
        public Condition Condition { get; set; }

        [JsonProperty("transport")]
        public WebSocketTransport Transport { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
    }
}
