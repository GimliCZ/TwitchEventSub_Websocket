using Newtonsoft.Json;

namespace Twitch.EventSub.API.ConduitModels
{
    public class ConduitCreateResponse
    {
        [JsonProperty("data")]
        public List<CreateResponseBody> Data { get; set; }
    }
    public class CreateResponseBody
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("shard_count")]
        public int ShardCount { get; set; }
    }
}
