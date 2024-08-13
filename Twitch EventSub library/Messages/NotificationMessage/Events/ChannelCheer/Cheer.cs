using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.ChannelCheer
{
    public class Cheer
    {
        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [JsonProperty("bits")]
        public int Bits { get; set; }

        [JsonProperty("tier")]
        public int Tier { get; set; }
    }
}