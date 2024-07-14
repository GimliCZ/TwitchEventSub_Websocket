using Newtonsoft.Json;

namespace Twitch.EventSub.API.ConduitModels
{
    public class ConduitCreateRequest
    {
        [JsonProperty("shard_count")]
        public int? ShardCount { get; set; }
    }
}
