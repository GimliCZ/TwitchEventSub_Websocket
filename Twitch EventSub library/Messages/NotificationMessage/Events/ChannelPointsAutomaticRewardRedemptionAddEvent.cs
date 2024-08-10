using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelPointsAutomaticRewardRedemptionAddEvent : WebSocketNotificationEvent
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("user_login")]
        public string UserLogin { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("reward")]
        public RewardAutomatic Reward { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("user_input")]
        public string UserInput { get; set; }

        [JsonProperty("redeemed_at")]
        public string RedeemedAt { get; set; }
    }

    public class RewardAutomatic
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("cost")]
        public int Cost { get; set; }

        [JsonProperty("unlocked_emote")]
        public UnlockedEmote UnlockedEmote { get; set; }
    }

    public class UnlockedEmote
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}