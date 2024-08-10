using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelUpdateEvent : WebSocketNotificationEvent
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("category_id")]
        public string CategoryId { get; set; }

        [JsonProperty("category_name")]
        public string CategoryName { get; set; }

        [JsonProperty("content_classification_labels")]
        public List<string> ContentClassificationLabels { get; set; }
    }
}