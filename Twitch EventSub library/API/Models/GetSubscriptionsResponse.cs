using Newtonsoft.Json;
using Twitch_EventSub_library.Messages.SharedContents;

namespace Twitch_EventSub_library.API.Models
{
    public class GetSubscriptionsResponse
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("data")]
        public List<WebSocketSubscription> Data { get; set; }

        [JsonProperty("total_cost")]
        public int TotalCost { get; set; }

        [JsonProperty("max_total_cost")]
        public int MaxTotalCost { get; set; }

        [JsonProperty("pagination")]
        public object Pagination { get; set; }

        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }
}
