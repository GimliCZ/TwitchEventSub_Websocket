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
        public string Cheermote { get; set; }

        [JsonProperty("emote")]
        public Emote Emote { get; set; }

        [JsonProperty("mention")]
        public string Mention { get; set; }
    }

}
