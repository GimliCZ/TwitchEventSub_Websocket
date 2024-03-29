﻿using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class Emote
    {
        [JsonProperty("begin")]
        public int Begin { get; set; }

        [JsonProperty("end")]
        public int End { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class SubscriptionMessageEvent : WebSocketNotificationEvent
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_login")]
        public string UserLogin { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("tier")]
        public string Tier { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("cumulative_months")]
        public int CumulativeMonths { get; set; }

        [JsonProperty("streak_months")]
        public int? StreakMonths { get; set; }

        [JsonProperty("duration_months")]
        public int DurationMonths { get; set; }
    }

    public class Message
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("emotes")]
        public List<Emote> Emotes { get; set; }

        [JsonProperty("fragments")]
        public List<Fragment> Fragments { get; set; }
    }
}
