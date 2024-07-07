using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ShoutoutCreateEvent : WebSocketNotificationEvent
    {
        [JsonProperty("moderator_user_id")]
        public string ModeratorUserId { get; set; }

        [JsonProperty("moderator_user_name")]
        public string ModeratorUserName { get; set; }

        [JsonProperty("moderator_user_login")]
        public string ModeratorUserLogin { get; set; }

        [JsonProperty("to_broadcaster_user_id")]
        public string ToBroadcasterUserId { get; set; }

        [JsonProperty("to_broadcaster_user_name")]
        public string ToBroadcasterUserName { get; set; }

        [JsonProperty("to_broadcaster_user_login")]
        public string ToBroadcasterUserLogin { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        [JsonProperty("viewer_count")]
        public int ViewerCount { get; set; }

        [JsonProperty("cooldown_ends_at")]
        public string CooldownEndsAt { get; set; }

        [JsonProperty("target_cooldown_ends_at")]
        public string TargetCooldownEndsAt { get; set; }
    }
}
