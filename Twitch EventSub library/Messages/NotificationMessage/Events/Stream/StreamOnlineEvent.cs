using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.Stream
{
    public class StreamOnlineEvent : WebSocketNotificationEvent
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("started_at")]
        public DateTime StartedAt { get; set; }
    }
}