﻿using Newtonsoft.Json;

namespace Twitch_EventSub_library.Messages.NotificationMessage.Events
{
    public class UnBanEvent : WebSocketNotificationEvent
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_login")]
        public string UserLogin { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; }

        [JsonProperty("broadcaster_user_login")]
        public string BroadcasterUserLogin { get; set; }

        [JsonProperty("broadcaster_user_name")]
        public string BroadcasterUserName { get; set; }

        [JsonProperty("moderator_user_id")]
        public string ModeratorUserId { get; set; }

        [JsonProperty("moderator_user_login")]
        public string ModeratorUserLogin { get; set; }

        [JsonProperty("moderator_user_name")]
        public string ModeratorUserName { get; set; }
    }
}
