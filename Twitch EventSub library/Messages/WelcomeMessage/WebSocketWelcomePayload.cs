using Newtonsoft.Json;
using Twitch_EventSub_library.Messages.SharedContents;

namespace Twitch_EventSub_library.Messages.WelcomeMessage
{
    public class WebSocketWelcomePayload
    {
        [JsonProperty("session")]
        public WebSocketSession Session { get; set; }
    }
}
