using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelUserChatClearEvent : WebSocketNotificationEvent
    {
        [JsonProperty("target_user_id")]
        public string UserId { get; set; }

        [JsonProperty("target_user_name")]
        public string UserName { get; set; }

        [JsonProperty("target_user_login")]
        public string UserLoginName { get; set; }
    }
}
