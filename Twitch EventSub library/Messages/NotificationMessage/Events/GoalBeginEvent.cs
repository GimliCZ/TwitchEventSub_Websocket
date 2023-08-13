﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Twitch.EventSub.Messages.NotificationMessage;

namespace Twitch.EventSub.Library.Messages.NotificationMessage.Events
{
    public class GoalBeginEvent:WebSocketNotificationEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("is_achieved")]
        public bool IsAchieved { get; set; }

        [JsonProperty("current_amount")]
        public int CurrentAmount { get; set; }

        [JsonProperty("target_amount")]
        public int TargetAmount { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        [JsonProperty("ended_at")]
        public string EndedAt { get; set; }
    }
}
