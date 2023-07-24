using Newtonsoft.Json;
using Twitch_EventSub_library.Messages.SharedContents;

namespace Twitch_EventSub_library.Messages.KeepAliveMessage
{
    public class WebSocketKeepAliveMessage: WebSocketMessage
    {
        [JsonProperty("payload")]
        public object? Payload { get; set; }
    }
}
