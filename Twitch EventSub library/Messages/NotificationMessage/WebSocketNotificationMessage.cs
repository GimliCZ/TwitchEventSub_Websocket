using Newtonsoft.Json;

namespace Twitch_EventSub_library.Messages.NotificationMessage
{
    public class WebSocketNotificationMessage : WebSocketMessage
    {
        [JsonProperty("payload")]
        public WebSocketNotificationPayload? Payload { get; set; }
    }
}
