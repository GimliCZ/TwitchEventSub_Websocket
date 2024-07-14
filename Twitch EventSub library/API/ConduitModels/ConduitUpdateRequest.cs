using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Twitch.EventSub.API.ConduitModels
{
    public class ConduitUpdateRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("shard_count")]
        public int? ShardCount { get; set; }
    }
}
