using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelSuspiciousUserMessageEvent : WebSocketNotificationEvent
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("user_login")]
        public string UserLogin { get; set; }

        [JsonProperty("low_trust_status")]
        public string LowTrustStatus { get; set; }

        [JsonProperty("shared_ban_channel_ids")]
        public List<string> SharedBanChannelIds { get; set; }

        [JsonProperty("types")]
        public List<string> Types { get; set; }

        [JsonProperty("ban_evasion_evaluation")]
        public string BanEvasionEvaluation { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }
    }
}