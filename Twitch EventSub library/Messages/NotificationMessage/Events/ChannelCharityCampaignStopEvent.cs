using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelCharityCampaignStopEvent : WebSocketNotificationEvent
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

        [JsonProperty("stopped_at")]
        public string StoppedAt { get; set; }
    }
}