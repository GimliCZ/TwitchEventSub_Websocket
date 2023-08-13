using Newtonsoft.Json;
using Twitch.EventSub.Messages.SharedContents;

namespace Twitch.EventSub.Messages.NotificationMessage
{
    public class WebSocketNotificationPayload
    {
        [JsonProperty("subscription")]
        public WebSocketSubscription? Subscription { get; set; }

        [JsonProperty("event")]
        public WebSocketNotificationEvent? Event { get; set; }
    }
}
