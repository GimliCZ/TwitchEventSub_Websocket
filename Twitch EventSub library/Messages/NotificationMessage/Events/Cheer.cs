using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class Cheer
    {
        [JsonProperty("prefix")]
        string Prefix { get; set; }

        [JsonProperty("bits")]
        int Bits { get; set; }

        [JsonProperty("tier")]
        int Tier { get; set; }
    }
}
