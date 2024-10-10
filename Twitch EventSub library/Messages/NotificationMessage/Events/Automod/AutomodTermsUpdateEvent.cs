using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.Automod
{
    public class AutomodTermsUpdateEvent : WebSocketNotificationEvent
    {
        [JsonProperty("moderator_user_id")]
        public string ModeratorUserId { get; set; }

        [JsonProperty("moderator_user_login")]
        public string ModeratorUserLogin { get; set; }

        [JsonProperty("moderator_user_name")]
        public string ModeratorUserName { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("from_automod")]
        public bool FromAutomod { get; set; }

        [JsonProperty("terms")]
        public List<string> Terms { get; set; }
    }
}