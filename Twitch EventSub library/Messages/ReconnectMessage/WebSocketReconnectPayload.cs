using Newtonsoft.Json;
using Twitch_EventSub_library.Messages.SharedContents;

namespace Twitch_EventSub_library.Messages.ReconnectMessage
{
    public class WebSocketReconnectPayload
    {
        [JsonProperty("session")]
        public WebSocketSession Session { get; set; }
    }
}
