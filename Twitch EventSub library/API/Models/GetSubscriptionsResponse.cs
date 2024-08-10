using Newtonsoft.Json;
using Twitch.EventSub.Messages.SharedContents;

namespace Twitch.EventSub.API.Models
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