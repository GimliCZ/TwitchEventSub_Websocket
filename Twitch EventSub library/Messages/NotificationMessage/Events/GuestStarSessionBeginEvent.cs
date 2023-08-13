using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class GuestStarSessionBeginEvent : WebSocketNotificationEvent
    {
        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }
    }
}
