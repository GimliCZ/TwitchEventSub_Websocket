using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelPointsCustomRewardRedemptionAddEvent : WebSocketNotificationEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_login")]
        public string UserLogin { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("user_input")]
        public string UserInput { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("reward")]
        public Reward Reward { get; set; }

        [JsonProperty("redeemed_at")]
        public string RedeemedAt { get; set; }
    }

    public class Reward
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("cost")]
        public int Cost { get; set; }

        [JsonProperty("prompt")]
        public string Prompt { get; set; }
    }
}