using Newtonsoft.Json;
using Twitch_EventSub_library.Messages.SharedContents;

namespace Twitch_EventSub_library.Messages.WelcomeMessage
{
    public class WebSocketWelcomeMessage: WebSocketMessage
    {
        [JsonProperty("payload")]
        public WebSocketWelcomePayload Payload { get; set; }
    }
}
