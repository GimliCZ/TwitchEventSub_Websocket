using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Twitch.EventSub.Messages.NotificationMessage;

namespace Twitch.EventSub.Library.Messages.NotificationMessage.Events
{
    public class ShoutoutReceivedEvent:WebSocketNotificationEvent
    {
        [JsonProperty("from_broadcaster_user_id")]
        public string FromBroadcasterUserId { get; set; }

        [JsonProperty("from_broadcaster_user_name")]
        public string FromBroadcasterUserName { get; set; }

        [JsonProperty("from_broadcaster_user_login")]
        public string FromBroadcasterUserLogin { get; set; }

        [JsonProperty("viewer_count")]
        public int ViewerCount { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }
    }
}
