using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelAdBreakBeginEvent : WebSocketNotificationEvent
    {
        [JsonProperty("duration_seconds")]
        public string DurationSeconds { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        [JsonProperty("is_automatic")]
        public string IsAutomatic { get; set; }

        [JsonProperty("requester_user_id")]
        public string RequesterUserId { get; set; }

        [JsonProperty("requester_user_login")]
        public string RequesterUserLogin { get; set; }

        [JsonProperty("requester_user_name")]
        public string RequesterUserName { get; set; }
    }
}