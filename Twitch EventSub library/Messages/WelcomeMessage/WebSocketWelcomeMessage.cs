using Newtonsoft.Json;

namespace Twitch_EventSub_library.Messages.WelcomeMessage
{
    public class WebSocketWelcomeMessage : WebSocketMessage
    {
        [JsonProperty("payload")]
        public WebSocketWelcomePayload? Payload { get; set; }
    }
}
