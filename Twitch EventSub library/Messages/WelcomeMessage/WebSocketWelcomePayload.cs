using Newtonsoft.Json;
using Twitch.EventSub.Messages.SharedContents;

namespace Twitch.EventSub.Messages.WelcomeMessage
{
    public class WebSocketWelcomePayload
    {
        [JsonProperty("session")]
        public WebSocketSession Session { get; set; }
    }
}