using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class GuestStarSessionEndEvent : GuestStarSessionBeginEvent
    {
        [JsonProperty("ended_at")]
        public DateTime EndedAt { get; set; }
    }
}
