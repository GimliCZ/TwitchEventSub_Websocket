using Newtonsoft.Json;
using Twitch.EventSub.Messages.NotificationMessage;

namespace Twitch.EventSub.Library.Messages.NotificationMessage.Events
{
    public class StreamOnlineEvent : WebSocketNotificationEvent
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("started_at")]
        public DateTime StartedAt { get; set; }
    }
}
