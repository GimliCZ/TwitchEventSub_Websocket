using Newtonsoft.Json;
using Twitch.EventSub.API.Models;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ConduitShardDisabledEvent : WebSocketNotificationEvent
    {
        [JsonProperty("conduit_id")]
        public string ConduitId;

        [JsonProperty("shard_id")]
        public string ShardId;

        [JsonProperty("status")]
        public string Status;

        [JsonProperty("transport")]
        public Transport Transport;
    }
}