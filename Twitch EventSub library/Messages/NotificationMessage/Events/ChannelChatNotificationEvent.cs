using Newtonsoft.Json;

namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelChatNotificationEvent : WebSocketNotificationEvent
    {
        [JsonProperty("chatter_user_id")]
        public string ChatterUserId { get; set; }

        [JsonProperty("chatter_user_login")]
        public string ChatterUserLogin { get; set; }

        [JsonProperty("chatter_user_name")]
        public string ChatterUserName { get; set; }

        [JsonProperty("chatter_is_anonymous")]
        public bool ChatterIsAnonymous { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("badges")]
        public List<Badge> Badges { get; set; }

        [JsonProperty("system_message")]
        public string SystemMessage { get; set; }

        [JsonProperty("message_id")]
        public string MessageId { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("notice_type")]
        public string NoticeType { get; set; }

        [JsonProperty("sub")]
        public SubEvent Sub { get; set; }

        [JsonProperty("resub")]
        public ResubEvent Resub { get; set; }

        [JsonProperty("sub_gift")]
        public SubGift SubGift { get; set; }

        [JsonProperty("community_sub_gift")]
        public CommunitySubGift CommunitySubGift { get; set; }

        [JsonProperty("gift_paid_upgrade")]
        public string GiftPaidUpgrade { get; set; }

        [JsonProperty("prime_paid_upgrade")]
        public string PrimePaidUpgrade { get; set; }

        [JsonProperty("pay_it_forward")]
        public string PayItForward { get; set; }

        [JsonProperty("raid")]
        public string Raid { get; set; }

        [JsonProperty("unraid")]
        public string Unraid { get; set; }

        [JsonProperty("announcement")]
        public Announcement Announcement { get; set; }

        [JsonProperty("bits_badge_tier")]
        public string BitsBadgeTier { get; set; }

        [JsonProperty("charity_donation")]
        public string CharityDonation { get; set; }
    }
    public class MessageFragment
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class SubEvent
    {
        [JsonProperty("sub_tier")]
        public string SubTier { get; set; }

        [JsonProperty("is_prime")]
        public bool IsPrime { get; set; }

        [JsonProperty("duration_months")]
        public int DurationMonths { get; set; }
    }

    public class ResubEvent
    {
        [JsonProperty("cumulative_months")]
        public int CumulativeMonths { get; set; }

        [JsonProperty("duration_months")]
        public int DurationMonths { get; set; }

        [JsonProperty("streak_months")]
        public int StreakMonths { get; set; }

        [JsonProperty("sub_tier")]
        public string SubTier { get; set; }

        [JsonProperty("is_prime")]
        public bool? IsPrime { get; set; }

        [JsonProperty("is_gift")]
        public bool IsGift { get; set; }

        [JsonProperty("gifter_is_anonymous")]
        public bool? GifterIsAnonymous { get; set; }

        [JsonProperty("gifter_user_id")]
        public string GifterUserId { get; set; }

        [JsonProperty("gifter_user_name")]
        public string GifterUserName { get; set; }

        [JsonProperty("gifter_user_login")]
        public string GifterUserLogin { get; set; }
    }

    public class SubGift
    {
        [JsonProperty("duration_months")]
        public int DurationMonths { get; set; }

        [JsonProperty("cumulative_total")]
        public int CumulativeTotal { get; set; }

        [JsonProperty("recipient_user_id")]
        public string RecipientUserId { get; set; }

        [JsonProperty("recipient_user_name")]
        public string RecipientUserName { get; set; }

        [JsonProperty("recipient_user_login")]
        public string RecipientUserLogin { get; set; }

        [JsonProperty("sub_tier")]
        public string SubTier { get; set; }

        [JsonProperty("community_gift_id")]
        public string CommunityGiftId { get; set; }
    }

    public class CommunitySubGift
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("sub_tier")]
        public string SubTier { get; set; }

        [JsonProperty("cumulative_total")]
        public int CumulativeTotal { get; set; }
    }
}
