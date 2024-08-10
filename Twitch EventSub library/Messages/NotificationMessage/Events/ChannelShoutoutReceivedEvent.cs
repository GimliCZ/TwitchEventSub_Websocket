using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelShoutoutReceivedEvent : WebSocketNotificationEvent
    {
        [JsonProperty("from_broadcaster_user_id")]
        public string FromBroadcasterUserId { get; set; }

        [JsonProperty("from_broadcaster_user_name")]
        public string FromBroadcasterUserName { get; set; }

        [JsonProperty("from_broadcaster_user_login")]
        public string FromBroadcasterUserLogin { get; set; }

        [JsonProperty("viewer_count")]
        public int ViewerCount { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }
    }
}