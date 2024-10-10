using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.WelcomeMessage
{
    public class WebSocketWelcomeMessage : WebSocketMessage
    {
        [JsonProperty("payload")]
        public WebSocketWelcomePayload? Payload { get; set; }
    }
}