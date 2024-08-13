using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.ChannelPrediction
{
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