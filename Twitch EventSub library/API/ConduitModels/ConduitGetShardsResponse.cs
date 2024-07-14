using Newtonsoft.Json;
using Twitch.EventSub.API.Models;

namespace Twitch.EventSub.API.ConduitModels
{
    public class ConduitGetShardsResponse
    {
        [JsonProperty("data")]
        public List<GetShardResponseBody> Data { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }

    }
    public class Pagination
    {
        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }
    public class GetShardResponseBody
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("transport")]
        public Transport Transport { get; set; }
    }
}
