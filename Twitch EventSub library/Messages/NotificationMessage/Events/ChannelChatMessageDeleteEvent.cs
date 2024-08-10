using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelChatMessageDeleteEvent : WebSocketNotificationEvent
    {
        [JsonProperty("target_user_id")]
        public string UserId { get; set; }

        [JsonProperty("target_user_login")]
        public string UserLogin { get; set; }

        [JsonProperty("target_user_name")]
        public string UserName { get; set; }

        [JsonProperty("message_id")]
        public string MessageId { get; set; }
    }
}