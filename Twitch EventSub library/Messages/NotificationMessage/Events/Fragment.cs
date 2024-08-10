using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class Fragment
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("cheermote")]
        public Cheermote Cheermote { get; set; }

        [JsonProperty("emote")]
        public Emote Emote { get; set; }

        [JsonProperty("mention")]
        public string Mention { get; set; }
    }

    public class Cheermote
    {
        public string Prefix { get; set; }
        public int Bits { get; set; }
        public int Tier { get; set; }
    }
}