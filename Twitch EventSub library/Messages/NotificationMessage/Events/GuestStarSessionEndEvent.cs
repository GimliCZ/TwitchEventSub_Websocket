using Newtonsoft.Json;

namespace Twitch_EventSub_library.Messages.NotificationMessage.Events
{
    public class GuestStarSessionEndEvent : GuestStarSessionBeginEvent
    {
        [JsonProperty("ended_at")]
        public DateTime EndedAt { get; set; }
    }
}
