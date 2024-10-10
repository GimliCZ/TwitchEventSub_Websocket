using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.NotificationEventClasses
{
    public class Announcement
    {
        [JsonProperty("color")]
        public string Color { get; set; }
    }
}