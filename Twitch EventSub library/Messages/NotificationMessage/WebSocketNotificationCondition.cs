using Newtonsoft.Json;

namespace Twitch_EventSub_library.Messages.NotificationMessage
{
    public class WebSocketNotificationCondition
    {
        [JsonProperty("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; }

    }
}