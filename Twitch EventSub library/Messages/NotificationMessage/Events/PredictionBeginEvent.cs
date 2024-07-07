using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class PredictionBeginEvent : WebSocketNotificationEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("outcomes")]
        public List<Outcome> Outcomes { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        [JsonProperty("locks_at")]
        public string LocksAt { get; set; }
    }
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
    public class TopPredictor
    {
        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("user_login")]
        public string UserLogin { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("channel_points_won")]
        public int? ChannelPointsWon { get; set; }

        [JsonProperty("channel_points_used")]
        public int? ChannelPointsUsed { get; set; }
    }
}
