using Newtonsoft.Json;

namespace Twitch_EventSub_library.Messages.SharedContents
{
    public class WebSocketExtendedMetadata
    {
        [JsonProperty("message_id")] public string MessageId { get; set; }

        [JsonProperty("message_type")] public string MessageType { get; set; }

        [JsonProperty("message_timestamp")] public string MessageTimestamp { get; set; }

        [JsonProperty("subscription_type")] public string SubscriptionType { get; set; }

        [JsonProperty("subscription_version")] public string SubscriptionVersion { get; set; }

    }
}

