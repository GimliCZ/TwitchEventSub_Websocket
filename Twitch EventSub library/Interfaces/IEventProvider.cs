using Twitch.EventSub.API.Models;
using Twitch.EventSub.CoreFunctions;
using Twitch.EventSub.Messages.NotificationMessage.Events;

namespace Twitch.EventSub.Interfaces
{
    public interface IEventProvider
    {
        event AsyncEventHandler<BanEvent> OnBanEventAsync;
        event AsyncEventHandler<FollowEvent> OnFollowEventAsync;
        event AsyncEventHandler<GoalBeginEvent> OnGoalBeginEventAsync;
        event AsyncEventHandler<GoalEndEvent> OnGoalEndEventAsync;
        event AsyncEventHandler<GoalProgressEvent> OnGoalProgressEventAsync;
        event AsyncEventHandler<GuestStarGuestUpdateEvent> OnGuestStarGuestUpdateEventAsync;
        event AsyncEventHandler<GuestStarSessionBeginEvent> OnGuestStarSessionBeginEventAsync;
        event AsyncEventHandler<GuestStarSessionEndEvent> OnGuestStarSessionEndEventAsync;
        event AsyncEventHandler<GuestStarSettingsUpdateEvent> OnGuestStarSettingsUpdateEventAsync;
        event AsyncEventHandler<GuestStarSlotUpdateEvent> OnGuestStarSlotUpdateEventAsync;
        event AsyncEventHandler<HypeTrainBeginEvent> OnHypeTrainBeginEventAsync;
        event AsyncEventHandler<HypeTrainEndEvent> OnHypeTrainEndEventAsync;
        event AsyncEventHandler<HypeTrainProgressEvent> OnHypeTrainProgressEventAsync;
        event AsyncEventHandler<ChannelChatMessage> OnChannelChatEventAsync;
        event AsyncEventHandler<CharityCampaignProgressEvent> OnCharityCampaignProgressEventAsync;
        event AsyncEventHandler<CharityCampaignStartEvent> OnCharityCampaignStartEventAsync;
        event AsyncEventHandler<CharityCampaignStopEvent> OnCharityCampaignStopEventAsync;
        event AsyncEventHandler<CharityDonationEvent> OnCharityDonationEventAsync;
        event AsyncEventHandler<CheerEvent> OnCheerEventAsync;
        event AsyncEventHandler<ModeratorAddEvent> OnModeratorAddEventAsync;
        event AsyncEventHandler<ModeratorRemoveEvent> OnModeratorRemoveEventAsync;
        event AsyncEventHandler<PointsCustomRewardAddEvent> OnPointsCustomRewardAddEventAsync;
        event AsyncEventHandler<PointsCustomRewardRedemptionAddEvent> OnPointsCustomRewardRedemptionAddEventAsync;
        event AsyncEventHandler<PointsCustomRewardRedemptionUpdateEvent> OnPointsCustomRewardRedemptionUpdateEventAsync;
        event AsyncEventHandler<PointsCustomRewardRemoveEvent> OnPointsCustomRewardRemoveEventAsync;
        event AsyncEventHandler<PointsCustomRewardUpdateEvent> OnPointsCustomRewardUpdateEventAsync;
        event AsyncEventHandler<PollBeginEvent> OnPollBeginEventAsync;
        event AsyncEventHandler<PollEndEvent> OnPollEndEventAsync;
        event AsyncEventHandler<PollProgressEvent> OnPollProgressEventAsync;
        event AsyncEventHandler<PredictionBeginEvent> OnPredictionBeginEventAsync;
        event AsyncEventHandler<PredictionEndEvent> OnPredictionEndEventAsync;
        event AsyncEventHandler<PredictionLockEvent> OnPredictionLockEventAsync;
        event AsyncEventHandler<PredictionProgressEvent> OnPredictionProgressEventAsync;
        event AsyncEventHandler<RaidEvent> OnRaidEventAsync;
        event AsyncEventHandler<string?> OnRawMessageAsync;
        event AsyncEventHandler<InvalidAccessTokenException> OnRefreshTokenAsync;
        event AsyncEventHandler<ShieldModeBeginEvent> OnShieldModeBeginEventAsync;
        event AsyncEventHandler<ShieldModeEndEvent> OnShieldModeEndEventAsync;
        event AsyncEventHandler<ShoutoutCreateEvent> OnShoutoutCreateEventAsync;
        event AsyncEventHandler<ShoutoutReceivedEvent> OnShoutoutReceivedEventAsync;
        event AsyncEventHandler<StreamOfflineEvent> OnStreamOfflineEventAsync;
        event AsyncEventHandler<StreamOnlineEvent> OnStreamOnlineEventAsync;
        event AsyncEventHandler<SubscribeEndEvent> OnSubscribeEndEventAsync;
        event AsyncEventHandler<SubscribeEvent> OnSubscribeEventAsync;
        event AsyncEventHandler<SubscriptionGiftEvent> OnSubscriptionGiftEventAsync;
        event AsyncEventHandler<SubscriptionMessageEvent> OnSubscriptionMessageEventAsync;
        event AsyncEventHandler<UnBanEvent> OnUnBanEventAsync;
        event EventHandler<string?> OnUnexpectedConnectionTermination;
        event AsyncEventHandler<UpdateNotificationEvent> OnUpdateNotificationEventAsync;

        Task StartAsync();
        Task <bool> StopAsync();
        bool IsDisposed();
        bool Update(string accessToken, List<SubscriptionType> listOfSubs);
    }
}