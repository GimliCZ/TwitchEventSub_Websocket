using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage
{
    public class WebSocketNotificationMessage : WebSocketMessage
    {
        [JsonProperty("payload")]
        public WebSocketNotificationPayload? Payload { get; set; }
    }
}
