using Newtonsoft.Json;
using Twitch.EventSub.Messages.NotificationMessage.Events.ChannelCheer;
using Twitch.EventSub.Messages.NotificationMessage.Events.ChannelSubscription;

namespace Twitch.EventSub.Messages.NotificationMessage.Events.ChannelChat
{
    public class ChannelChatMessageEvent : WebSocketNotificationEvent
    {
        [JsonProperty("chatter_user_id")]
        public string ChatterUserId { get; set; }

        [JsonProperty("chatter_user_login")]
        public string ChatterUserLogin { get; set; }

        [JsonProperty("chatter_user_name")]
        public string ChatterUserName { get; set; }

        [JsonProperty("message_id")]
        public string MessageId { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("badges")]
        public List<Badge> Badges { get; set; }

        [JsonProperty("message_type")]
        public string MessageType { get; set; }

        [JsonProperty("cheer")]
        public Cheer Cheer { get; set; }

        [JsonProperty("emote")]
        public List<MessageEmote> Emote { get; set; }

        [JsonProperty("reply")]
        public List<Reply> Reply { get; set; }

        [JsonProperty("channel_points_custom_reward_id")]
        public string ChannelPointsCustomRewardId { get; set; }

        [JsonProperty("channel_points_animation_id")]
        public string ChannelPointsAnimationId { get; set; }
    }
}