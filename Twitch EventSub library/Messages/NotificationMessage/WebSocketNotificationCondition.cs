using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage
{
    public class WebSocketNotificationCondition
    {
        [JsonProperty("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; }

    }
}