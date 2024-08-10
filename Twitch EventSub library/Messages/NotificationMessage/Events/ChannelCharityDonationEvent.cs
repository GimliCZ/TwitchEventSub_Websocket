using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelCharityDonationEvent : WebSocketNotificationEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("campaign_id")]
        public string CampaignId { get; set; }

        [JsonProperty("charity_name")]
        public string CharityName { get; set; }

        [JsonProperty("charity_description")]
        public string CharityDescription { get; set; }

        [JsonProperty("charity_logo")]
        public string CharityLogo { get; set; }

        [JsonProperty("charity_website")]
        public string CharityWebsite { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        [JsonProperty("stopped_at")]
        public string StoppedAt { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_login")]
        public string UserLogin { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("amount")]
        public Amount Amount { get; set; }
    }

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