using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class SubscriptionGiftEvent : WebSocketNotificationEvent
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_login")]
        public string UserLogin { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("tier")]
        public string Tier { get; set; }

        [JsonProperty("cumulative_total")] //MAY BE NULL
        public int? CumulativeTotal { get; set; }

        [JsonProperty("is_anonymous")]
        public bool IsAnonymous { get; set; }
    }
}
