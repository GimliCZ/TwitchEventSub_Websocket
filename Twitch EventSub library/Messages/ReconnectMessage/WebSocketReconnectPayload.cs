using Newtonsoft.Json;
using Twitch.EventSub.Messages.SharedContents;

namespace Twitch.EventSub.Messages.ReconnectMessage
{
    public class WebSocketReconnectPayload
    {
        [JsonProperty("session")]
        public WebSocketSession Session { get; set; }
    }
}