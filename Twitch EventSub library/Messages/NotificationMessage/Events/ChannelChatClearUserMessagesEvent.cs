using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelChatClearUserMessagesEvent : WebSocketNotificationEvent
    {
        [JsonProperty("target_user_id")]
        public string TargetUserId { get; set; }

        [JsonProperty("target_user_name")]
        public string TargetUserName { get; set; }

        [JsonProperty("target_user_login")]
        public string TargetUserLogin { get; set; }
    }
}