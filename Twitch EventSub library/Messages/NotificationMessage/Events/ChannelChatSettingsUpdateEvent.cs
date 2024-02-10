using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelChatSettingsUpdateEvent : WebSocketNotificationEvent
    {
        [JsonProperty("emote_mode")]
        public bool EmoteMode { get; set; }

        [JsonProperty("follower_mode")]
        public bool FollowerMode { get; set; }

        [JsonProperty("follower_mode_duration_minutes")]
        public object FollowerModeDurationMinutes { get; set; }

        [JsonProperty("slow_mode")]
        public bool SlowMode { get; set; }

        [JsonProperty("slow_mode_wait_time_seconds")]
        public int SlowModeWaitTimeSeconds { get; set; }

        [JsonProperty("subscriber_mode")]
        public bool SubscriberMode { get; set; }

        [JsonProperty("unique_chat_mode")]
        public bool UniqueChatMode { get; set; }
    }
}
