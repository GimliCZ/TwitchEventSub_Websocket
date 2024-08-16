using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.ChannelPoll
{
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
}