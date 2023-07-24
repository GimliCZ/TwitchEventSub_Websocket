namespace Twitch_EventSub_library.API.Models
{
    public class SubscriptionTypes
    {
        public enum SubscriptionType
        {
            // Channel Subscriptions
            ChannelUpdate,
            ChannelFollow,
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
}
