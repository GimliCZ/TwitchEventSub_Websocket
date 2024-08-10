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
        public MessageNotification Message { get; set; }

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
        public GiftPaidUpgradeNotification GiftPaidUpgrade { get; set; }

        [JsonProperty("prime_paid_upgrade")]
        public PrimePaidUpgradeNotification PrimePaidUpgrade { get; set; }

        [JsonProperty("pay_it_forward")]
        public PayItForwardNotification PayItForward { get; set; }

        [JsonProperty("raid")]
        public RaidNotification Raid { get; set; }

        [JsonProperty("unraid")]
        public string Unraid { get; set; }

        [JsonProperty("announcement")]
        public Announcement Announcement { get; set; }

        [JsonProperty("bits_badge_tier")]
        public BitsBadgeTierNotification BitsBadgeTier { get; set; }

        [JsonProperty("charity_donation")]
        public CharityDonationNotification CharityDonation { get; set; }
    }

    public class MessageNotification
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("fragments")]
        public List<FragmentNotification> Fragments { get; set; }
    }

    public class FragmentNotification
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("cheermote")]
        public CheermoteNotification Cheermote { get; set; }

        [JsonProperty("emote")]
        public EmoteNotification Emote { get; set; }

        [JsonProperty("mention")]
        public MentionNotification Mention { get; set; }
    }

    public class EmoteNotification
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("emote_set_id")]
        public string EmoteSetId { get; set; }

        [JsonProperty("owner_id")]
        public string OwnerId { get; set; }

        [JsonProperty("format")]
        public List<string> Format { get; set; }
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

    public class CheermoteNotification
    {
        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [JsonProperty("bits")]
        public int Bits { get; set; }

        [JsonProperty("tier")]
        public int Tier { get; set; }
    }

    public class MentionNotification
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("user_login")]
        public string UserLogin { get; set; }
    }

    public class GiftPaidUpgradeNotification
    {
        [JsonProperty("gifter_is_anonymous")]
        public bool GifterIsAnonymous { get; set; }

        [JsonProperty("gifter_user_id")]
        public string GifterUserId { get; set; }

        [JsonProperty("gifter_user_name")]
        public string GifterUserName { get; set; }

        [JsonProperty("gifter_user_login")]
        public string GifterUserLogin { get; set; }
    }

    public class PrimePaidUpgradeNotification
    {
        [JsonProperty("sub_tier")]
        public string SubTier { get; set; }
    }

    public class PayItForwardNotification
    {
        [JsonProperty("gifter_is_anonymous")]
        public bool GifterIsAnonymous { get; set; }

        [JsonProperty("gifter_user_id")]
        public string GifterUserId { get; set; }

        [JsonProperty("gifter_user_name")]
        public string GifterUserName { get; set; }

        [JsonProperty("gifter_user_login")]
        public string GifterUserLogin { get; set; }
    }

    public class RaidNotification
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("user_login")]
        public string UserLogin { get; set; }

        [JsonProperty("viewer_count")]
        public int ViewerCount { get; set; }

        [JsonProperty("profile_image_url")]
        public string ProfileImageUrl { get; set; }
    }

    public class BitsBadgeTierNotification
    {
        [JsonProperty("tier")]
        public int Tier { get; set; }
    }

    public class CharityDonationNotification
    {
        [JsonProperty("charity_name")]
        public string CharityName { get; set; }

        [JsonProperty("amount")]
        public DonationAmount Amount { get; set; }
    }

    public class DonationAmount
    {
        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("decimal_place")]
        public int DecimalPlace { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
}