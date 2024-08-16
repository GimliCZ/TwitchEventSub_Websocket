using Newtonsoft.Json;
using Twitch.EventSub.Messages.NotificationMessage.Events.ChannelSubscription;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.ChannelChat
{
    public class ChannelChatUserMessageUpdateEvent : WebSocketNotificationEvent
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_login")]
        public string UserLogin { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("message_id")]
        public string MessageId { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }
    }
}