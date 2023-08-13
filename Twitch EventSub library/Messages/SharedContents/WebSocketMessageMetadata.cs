using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.SharedContents
{
    public class WebSocketMessageMetadata
    {
        [JsonProperty("message_id")]
        public string MessageId { get; set; }

        [JsonProperty("message_type")]
        public string MessageType { get; set; }

        [JsonProperty("message_timestamp")]
        public string MessageTimestamp { get; set; }
    }
}
