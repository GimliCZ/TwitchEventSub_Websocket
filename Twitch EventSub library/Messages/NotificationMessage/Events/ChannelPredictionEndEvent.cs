using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelPredictionEndEvent : ChannelPredictionBeginEvent
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("ended_at")]
        public string EndedAt { get; set; }
    }
}