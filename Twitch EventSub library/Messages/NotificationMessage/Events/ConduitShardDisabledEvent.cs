using Newtonsoft.Json;
using Twitch.EventSub.API.Models;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ConduitShardDisabledEvent : WebSocketNotificationEvent
    {
        [JsonProperty("conduit_id")]
        public string ConduitId { get; set; }

        [JsonProperty("shard_id")]
        public string ShardId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("transport")]
        public Transport Transport { get; set; }
    }
}