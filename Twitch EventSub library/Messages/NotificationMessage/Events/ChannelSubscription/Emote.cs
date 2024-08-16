using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.ChannelSubscription
{
    public class Emote
    {
        [JsonProperty("begin")]
        public int Begin { get; set; }

        [JsonProperty("end")]
        public int End { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("emote_set_id")]
        public string EmoteSetId { get; set; }
    }
}