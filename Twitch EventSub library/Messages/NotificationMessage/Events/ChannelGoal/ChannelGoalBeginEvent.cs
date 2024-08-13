using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.ChannelGoal
{
    public class ChannelGoalBeginEvent : WebSocketNotificationEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("current_amount")]
        public int CurrentAmount { get; set; }

        [JsonProperty("target_amount")]
        public int TargetAmount { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }
    }
}