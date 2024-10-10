using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.ChannelPoll
{
    public class BitsVoting
    {
        [JsonProperty("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("amount_per_vote")]
        public int AmountPerVote { get; set; }
    }
}