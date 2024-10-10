using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.ChannelHype
{
    public class ChannelHypeTrainBeginEvent : WebSocketNotificationEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("progress")]
        public int Progress { get; set; }

        [JsonProperty("goal")]
        public int Goal { get; set; }

        [JsonProperty("top_contributions")]
        public List<TopContribution> TopContributions { get; set; }

        [JsonProperty("last_contribution")]
        public LastContribution LastContribution { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        [JsonProperty("expires_at")]
        public string ExpiresAt { get; set; }
    }
}