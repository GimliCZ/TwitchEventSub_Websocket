using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class DefaultImage
    {
        [JsonProperty("url_1x")]
        public string Url1x { get; set; }

        [JsonProperty("url_2x")]
        public string Url2x { get; set; }

        [JsonProperty("url_4x")]
        public string Url4x { get; set; }
    }

    public class PointsCustomRewardAddEvent : WebSocketNotificationEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("is_paused")]
        public bool IsPaused { get; set; }

        [JsonProperty("is_in_stock")]
        public bool IsInStock { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("cost")]
        public int Cost { get; set; }

        [JsonProperty("prompt")]
        public string Prompt { get; set; }

        [JsonProperty("is_user_input_required")]
        public bool IsUserInputRequired { get; set; }

        [JsonProperty("should_redemptions_skip_request_queue")]
        public bool ShouldRedemptionsSkipRequestQueue { get; set; }

        [JsonProperty("cooldown_expires_at")]
        public object CooldownExpiresAt { get; set; }

        [JsonProperty("redemptions_redeemed_current_stream")]
        public object RedemptionsRedeemedCurrentStream { get; set; }

        [JsonProperty("max_per_stream")]
        public MaxPerStream MaxPerStream { get; set; }

        [JsonProperty("max_per_user_per_stream")]
        public MaxPerUserPerStream MaxPerUserPerStream { get; set; }

        [JsonProperty("global_cooldown")]
        public GlobalCooldown GlobalCooldown { get; set; }

        [JsonProperty("background_color")]
        public string BackgroundColor { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("default_image")]
        public DefaultImage DefaultImage { get; set; }
    }

    public class GlobalCooldown
    {
        [JsonProperty("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("seconds")]
        public int Seconds { get; set; }
    }

    public class Image
    {
        [JsonProperty("url_1x")]
        public string Url1x { get; set; }

        [JsonProperty("url_2x")]
        public string Url2x { get; set; }

        [JsonProperty("url_4x")]
        public string Url4x { get; set; }
    }

    public class MaxPerStream
    {
        [JsonProperty("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }
    }

    public class MaxPerUserPerStream
    {
        [JsonProperty("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }
    }
}
