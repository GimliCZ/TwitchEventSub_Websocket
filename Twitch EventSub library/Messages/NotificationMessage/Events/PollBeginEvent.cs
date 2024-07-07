using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class PollBeginEvent : WebSocketNotificationEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("choices")]
        public List<Choice> Choices { get; set; }

        [JsonProperty("bits_voting")]
        public BitsVoting BitsVoting { get; set; }

        [JsonProperty("channel_points_voting")]
        public ChannelPointsVoting ChannelPointsVoting { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        [JsonProperty("ends_at")]
        public string EndsAt { get; set; }
    }
    public class BitsVoting
    {
        [JsonProperty("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("amount_per_vote")]
        public int AmountPerVote { get; set; }
    }
    public class Choice
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("bits_votes")]
        public int? BitsVotes { get; set; }

        [JsonProperty("channel_points_votes")]
        public int? ChannelPointsVotes { get; set; }

        [JsonProperty("votes")]
        public int? Votes { get; set; }
    }
    public class ChannelPointsVoting
    {
        [JsonProperty("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("amount_per_vote")]
        public int AmountPerVote { get; set; }
    }
}
