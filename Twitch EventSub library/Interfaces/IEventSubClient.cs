using Twitch.EventSub.API.Models;
using Twitch.EventSub.CoreFunctions;
using Twitch.EventSub.Library.Messages.NotificationMessage.Events;
using Twitch.EventSub.Messages.NotificationMessage.Events;
using Twitch.EventSub.User;

namespace Twitch.EventSub.Interfaces
{
    public interface IEventSubClient
    {
        event AsyncEventHandler<BanEvent, UserSequencer> OnBanEventAsync;
        event AsyncEventHandler<FollowEvent, UserSequencer> OnFollowEventAsync;
        event AsyncEventHandler<GoalBeginEvent, UserSequencer> OnGoalBeginEventAsync;
        event AsyncEventHandler<GoalEndEvent, UserSequencer> OnGoalEndEventAsync;
        event AsyncEventHandler<GoalProgressEvent, UserSequencer> OnGoalProgressEventAsync;
        event AsyncEventHandler<GuestStarGuestUpdateEvent, UserSequencer> OnGuestStarGuestUpdateEventAsync;
        event AsyncEventHandler<GuestStarSessionBeginEvent, UserSequencer> OnGuestStarSessionBeginEventAsync;
        event AsyncEventHandler<GuestStarSessionEndEvent, UserSequencer> OnGuestStarSessionEndEventAsync;
        event AsyncEventHandler<GuestStarSettingsUpdateEvent, UserSequencer> OnGuestStarSettingsUpdateEventAsync;
        event AsyncEventHandler<GuestStarSlotUpdateEvent, UserSequencer> OnGuestStarSlotUpdateEventAsync;
        event AsyncEventHandler<HypeTrainBeginEvent, UserSequencer> OnHypeTrainBeginEventAsync;
        event AsyncEventHandler<HypeTrainEndEvent, UserSequencer> OnHypeTrainEndEventAsync;
        event AsyncEventHandler<HypeTrainProgressEvent, UserSequencer> OnHypeTrainProgressEventAsync;
        event AsyncEventHandler<ChannelChatMessage, UserSequencer> OnChannelChatEventAsync;
        event AsyncEventHandler<CharityCampaignProgressEvent, UserSequencer> OnCharityCampaignProgressEventAsync;
        event AsyncEventHandler<CharityCampaignStartEvent, UserSequencer> OnCharityCampaignStartEventAsync;
        event AsyncEventHandler<CharityCampaignStopEvent, UserSequencer> OnCharityCampaignStopEventAsync;
        event AsyncEventHandler<CharityDonationEvent, UserSequencer> OnCharityDonationEventAsync;
        event AsyncEventHandler<CheerEvent, UserSequencer> OnCheerEventAsync;
        event AsyncEventHandler<ModeratorAddEvent, UserSequencer> OnModeratorAddEventAsync;
        event AsyncEventHandler<ModeratorRemoveEvent, UserSequencer> OnModeratorRemoveEventAsync;
        event AsyncEventHandler<PointsCustomRewardAddEvent, UserSequencer> OnPointsCustomRewardAddEventAsync;
        event AsyncEventHandler<PointsCustomRewardRedemptionAddEvent, UserSequencer> OnPointsCustomRewardRedemptionAddEventAsync;
        event AsyncEventHandler<PointsCustomRewardRedemptionUpdateEvent, UserSequencer> OnPointsCustomRewardRedemptionUpdateEventAsync;
        event AsyncEventHandler<PointsCustomRewardRemoveEvent, UserSequencer> OnPointsCustomRewardRemoveEventAsync;
        event AsyncEventHandler<PointsCustomRewardUpdateEvent, UserSequencer> OnPointsCustomRewardUpdateEventAsync;
        event AsyncEventHandler<PollBeginEvent, UserSequencer> OnPollBeginEventAsync;
        event AsyncEventHandler<PollEndEvent, UserSequencer> OnPollEndEventAsync;
        event AsyncEventHandler<PollProgressEvent, UserSequencer> OnPollProgressEventAsync;
        event AsyncEventHandler<PredictionBeginEvent, UserSequencer> OnPredictionBeginEventAsync;
        event AsyncEventHandler<PredictionEndEvent, UserSequencer> OnPredictionEndEventAsync;
        event AsyncEventHandler<PredictionLockEvent, UserSequencer> OnPredictionLockEventAsync;
        event AsyncEventHandler<PredictionProgressEvent, UserSequencer> OnPredictionProgressEventAsync;
        event AsyncEventHandler<RaidEvent, UserSequencer> OnRaidEventAsync;
        event AsyncEventHandler<string?, UserSequencer> OnRawMessageAsync;
        event AsyncEventHandler<InvalidAccessTokenException, UserSequencer> OnRefreshTokenAsync;
        event AsyncEventHandler<ShieldModeBeginEvent, UserSequencer> OnShieldModeBeginEventAsync;
        event AsyncEventHandler<ShieldModeEndEvent, UserSequencer> OnShieldModeEndEventAsync;
        event AsyncEventHandler<ShoutoutCreateEvent, UserSequencer> OnShoutoutCreateEventAsync;
        event AsyncEventHandler<ShoutoutReceivedEvent, UserSequencer> OnShoutoutReceivedEventAsync;
        event AsyncEventHandler<StreamOfflineEvent, UserSequencer> OnStreamOfflineEventAsync;
        event AsyncEventHandler<StreamOnlineEvent, UserSequencer> OnStreamOnlineEventAsync;
        event AsyncEventHandler<SubscribeEndEvent, UserSequencer> OnSubscribeEndEventAsync;
        event AsyncEventHandler<SubscribeEvent, UserSequencer> OnSubscribeEventAsync;
        event AsyncEventHandler<SubscriptionGiftEvent, UserSequencer> OnSubscriptionGiftEventAsync;
        event AsyncEventHandler<SubscriptionMessageEvent, UserSequencer> OnSubscriptionMessageEventAsync;
        event AsyncEventHandler<UnBanEvent, UserSequencer> OnUnBanEventAsync;
        event EventHandler<string?> OnUnexpectedConnectionTermination;
        event AsyncEventHandler<UpdateNotificationEvent, UserSequencer> OnUpdateNotificationEventAsync;

        Task<bool> AddUserAsync(string userId, string accessToken, List<SubscriptionType> listOfSubs);
        Task<bool> DeleteUserAsync(string userId);
        bool UpdateUser(string userId, string accessToken, List<SubscriptionType> listOfSubs);
    }
}