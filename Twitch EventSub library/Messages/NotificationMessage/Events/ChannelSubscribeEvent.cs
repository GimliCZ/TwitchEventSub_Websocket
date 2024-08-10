using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelSubscribeEvent : WebSocketNotificationEvent
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_login")]
        public string UserLogin { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("tier")]
        public string Tier { get; set; }

        [JsonProperty("is_gift")]
        public bool IsGift { get; set; }
    }
}