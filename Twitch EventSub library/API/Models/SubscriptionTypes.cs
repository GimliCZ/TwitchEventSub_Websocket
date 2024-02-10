namespace Twitch.EventSub.API.Models
{
    public enum SubscriptionType
    {
        // Channel Subscriptions
        ChannelUpdate,
        ChannelFollow,
        ChannelAdBreakBegin,
        ChannelChatClear,
        ChannelChatClearUserMessages,
        ChannelChatMessage,
        ChannelChatMessageDelete,
        ChannelChatNotification,
        ChannelSubscribe,
        ChannelSubscriptionEnd,
        ChannelSubscriptionGift,
        ChannelSubscriptionMessage,
        ChannelCheer,
        ChannelRaid,
        ChannelBan,
        ChannelUnban,
        ChannelModeratorAdd,
        ChannelModeratorRemove,

        // Beta Channel Guest Star
        BetaChannelChatSettingsUpdate,
        BetaChannelGuestStarSessionBegin,
        BetaChannelGuestStarSessionEnd,
        BetaChannelGuestStarGuestUpdate,
        BetaChannelGuestStarSlotUpdate,
        BetaChannelGuestStarSettingsUpdate,

        // Channel Points
        ChannelPointsCustomRewardAdd,
        ChannelPointsCustomRewardUpdate,
        ChannelPointsCustomRewardRemove,
        ChannelPointsCustomRewardRedemptionAdd,
        ChannelPointsCustomRewardRedemptionUpdate,

        // Channel Poll
        ChannelPollBegin,
        ChannelPollProgress,
        ChannelPollEnd,

        // Channel Prediction
        ChannelPredictionBegin,
        ChannelPredictionProgress,
        ChannelPredictionLock,
        ChannelPredictionEnd,

        // Charity
        CharityDonation,
        CharityCampaignStart,
        CharityCampaignProgress,
        CharityCampaignStop,

        //webhook only
        /* 
        // Drop Entitlement Grant
        DropEntitlementGrant,

        // Extension Bits Transaction
        ExtensionBitsTransactionCreate,
        */

        // Channel Goal
        ChannelGoalBegin,
        ChannelGoalProgress,
        ChannelGoalEnd,

        // Channel Hype Train
        ChannelHypeTrainBegin,
        ChannelHypeTrainProgress,
        ChannelHypeTrainEnd,

        // Channel Shield Mode
        ChannelShieldModeBegin,
        ChannelShieldModeEnd,

        // Channel Shoutout
        ChannelShoutoutCreate,
        ChannelShoutoutReceived,

        // Stream
        StreamOnline,
        StreamOffline,

        //webhook only
        /*
        // User Authorization
        UserAuthorizationGrant,
        UserAuthorizationRevoke,
        */

        // User Update
        UserUpdate
    }
}