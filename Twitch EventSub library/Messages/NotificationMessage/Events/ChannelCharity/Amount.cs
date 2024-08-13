using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.ChannelCharity
{
    public class Amount
    {
        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("decimal_places")]
        public int DecimalPlaces { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
}