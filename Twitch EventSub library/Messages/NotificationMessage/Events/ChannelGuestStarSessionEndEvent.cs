using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelGuestStarSessionEndEvent : WebSocketNotificationEvent
    {
        [JsonProperty("moderator_user_id")]
        public string ModeratorUserId { get; set; }

        [JsonProperty("moderator_user_name")]
        public string ModeratorUserName { get; set; }

        [JsonProperty("moderator_user_login")]
        public string ModeratorUserLogin { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        [JsonProperty("ended_at")]
        public string EndedAt { get; set; }
    }
}