using Newtonsoft.Json;
using Twitch_EventSub_library.Messages.SharedContents;

namespace Twitch_EventSub_library.Messages.NotificationMessage
{
    public class WebSocketNotificationPayload
    {
        [JsonProperty("subscription")]
        public WebSocketSubscription Subscription { get; set; }

        [JsonProperty("event")]
        public WebSocketNotificationEvent Event { get; set; }
    }
}
