using Newtonsoft.Json;
using Twitch.EventSub.Messages.NotificationMessage;

namespace Twitch.EventSub.Library.Messages.NotificationMessage.Events
{
    public class CharityDonationEvent:WebSocketNotificationEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("charity_name")]
        public string CharityName { get; set; }

        [JsonProperty("charity_description")]
        public string CharityDescription { get; set; }

        [JsonProperty("charity_logo")]
        public string CharityLogo { get; set; }

        [JsonProperty("charity_website")]
        public string CharityWebsite { get; set; }

        [JsonProperty("current_amount")]
        public CurrentAmount CurrentAmount { get; set; }

        [JsonProperty("target_amount")]
        public TargetAmount TargetAmount { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        [JsonProperty("stopped_at")]
        public string StoppedAt { get; set; }
    }
    public class TargetAmount
    {
        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("decimal_places")]
        public int DecimalPlaces { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
    public class CurrentAmount
    {
        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("decimal_places")]
        public int DecimalPlaces { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
}
