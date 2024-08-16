using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.ChannelPrediction
{
    public class ChannelPredictionEndEvent : ChannelPredictionBeginEvent
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("ended_at")]
        public string EndedAt { get; set; }
    }
}