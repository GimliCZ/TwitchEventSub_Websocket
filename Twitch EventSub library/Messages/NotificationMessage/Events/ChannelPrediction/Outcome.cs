using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.ChannelPrediction
{
    public class Outcome
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("users")]
        public int? Users { get; set; }

        [JsonProperty("channel_points")]
        public int? ChannelPoints { get; set; }

        [JsonProperty("top_predictors")]
        public List<TopPredictor> TopPredictors { get; set; }
    }
}