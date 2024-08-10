using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class Cheer
    {
        [JsonProperty("prefix")]
        private string Prefix { get; set; }

        [JsonProperty("bits")]
        private int Bits { get; set; }

        [JsonProperty("tier")]
        private int Tier { get; set; }
    }
}