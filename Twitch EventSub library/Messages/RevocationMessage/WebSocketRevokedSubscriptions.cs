using Newtonsoft.Json;
using Twitch.EventSub.Messages.SharedContents;

namespace Twitch.EventSub.Messages.RevocationMessage
{
    public class WebSocketRevokedSubscriptions
    {
        [JsonProperty("subscription")]
        public WebSocketSubscription? Subscription { get; set; }
    }
}
