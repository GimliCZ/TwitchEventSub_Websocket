﻿using Newtonsoft.Json;

namespace Twitch_EventSub_library.Messages.NotificationMessage.Events
{
    public class GuestStarGuestUpdateEvent : WebSocketNotificationEvent
    {
        [JsonProperty("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; }

        [JsonProperty("broadcaster_user_name")]
        public string BroadcasterUserName { get; set; }

        [JsonProperty("broadcaster_user_login")]
        public string BroadcasterUserLogin { get; set; }

        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        [JsonProperty("moderator_user_id")]
        public string ModeratorUserId { get; set; }

        [JsonProperty("moderator_user_name")]
        public string ModeratorUserName { get; set; }

        [JsonProperty("moderator_user_login")]
        public string ModeratorUserLogin { get; set; }

        [JsonProperty("guest_user_id")]
        public string GuestUserId { get; set; }

        [JsonProperty("guest_user_name")]
        public string GuestUserName { get; set; }

        [JsonProperty("guest_user_login")]
        public string GuestUserLogin { get; set; }

        [JsonProperty("slot_id")]
        public string SlotId { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }
}
