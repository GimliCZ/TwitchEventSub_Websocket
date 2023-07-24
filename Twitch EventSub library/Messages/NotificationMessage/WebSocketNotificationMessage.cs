using Newtonsoft.Json;
using Twitch_EventSub_library.Messages.SharedContents;

namespace Twitch_EventSub_library.Messages.NotificationMessage
{
    public class WebSocketNotificationMessage: WebSocketMessage
    {
        [JsonProperty("payload")]
        public WebSocketNotificationPayload Payload { get; set; }
    }
}
