﻿using Newtonsoft.Json;

namespace Twitch_EventSub_library.Messages.NotificationMessage.Events
{
    public class CheerEvent : WebSocketNotificationEvent
    {
        [JsonProperty("is_anonymous")]
        public bool IsAnonymous { get; set; }

        [JsonProperty("user_id")]
        public string? UserId { get; set; }

        [JsonProperty("user_login")]
        public string? UserLogin { get; set; }

        [JsonProperty("user_name")]
        public string? UserName { get; set; }

        [JsonProperty("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; }

        [JsonProperty("broadcaster_user_login")]
        public string BroadcasterUserLogin { get; set; }

        [JsonProperty("broadcaster_user_name")]
        public string BroadcasterUserName { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("bits")]
        public int Bits { get; set; }
    }
}
