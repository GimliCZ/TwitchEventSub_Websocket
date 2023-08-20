using Twitch.EventSub.API.Models;
using Twitch.EventSub.CoreFunctions;
using Twitch.EventSub.Library.Messages.NotificationMessage.Events;
using Twitch.EventSub.Messages.NotificationMessage.Events;

namespace Twitch.EventSub.Interfaces
{
    public interface IEventSubClient
    {
        event EventHandler<string?> OnUnexpectedConnectionTermination;

        event AsyncEventHandler<InvalidAccessTokenException> OnRefreshTokenAsync;

        event AsyncEventHandler<UpdateNotificationEvent> OnUpdateNotificationEventAsync;

        event AsyncEventHandler<FollowEvent> OnFollowEventAsync;

        event AsyncEventHandler<SubscribeEvent> OnSubscribeEventAsync;

        event AsyncEventHandler<SubscribeEndEvent> OnSubscribeEndEventAsync;

        event AsyncEventHandler<SubscriptionGiftEvent> OnSubscriptionGiftEventAsync;

        event AsyncEventHandler<SubscriptionMessageEvent> OnSubscriptionMessageEventAsync;

        event AsyncEventHandler<CheerEvent> OnCheerEventAsync;

        event AsyncEventHandler<RaidEvent> OnRaidEventAsync;

        event AsyncEventHandler<BanEvent> OnBanEventAsync;

        event AsyncEventHandler<UnBanEvent> OnUnBanEventAsync;

        event AsyncEventHandler<ModeratorAddEvent> OnModeratorAddEventAsync;

        event AsyncEventHandler<ModeratorRemoveEvent> OnModeratorRemoveEventAsync;

        event AsyncEventHandler<GuestStarSessionBeginEvent> OnGuestStarSessionBeginEventAsync;

        event AsyncEventHandler<GuestStarSessionEndEvent> OnGuestStarSessionEndEventAsync;

        event AsyncEventHandler<GuestStarGuestUpdateEvent> OnGuestStarGuestUpdateEventAsync;

        event AsyncEventHandler<GuestStarSlotUpdateEvent> OnGuestStarSlotUpdateEventAsync;

        event AsyncEventHandler<GuestStarSettingsUpdateEvent> OnGuestStarSettingsUpdateEventAsync;

        event AsyncEventHandler<PointsCustomRewardAddEvent> OnPointsCustomRewardAddEventAsync;

        event AsyncEventHandler<PointsCustomRewardUpdateEvent> OnPointsCustomRewardUpdateEventAsync;

        event AsyncEventHandler<PointsCustomRewardRemoveEvent> OnPointsCustomRewardRemoveEventAsync;

        event AsyncEventHandler<PointsCustomRewardRedemptionAddEvent> OnPointsCustomRewardRedemptionAddEventAsync;

        event AsyncEventHandler<PointsCustomRewardRedemptionUpdateEvent> OnPointsCustomRewardRedemptionUpdateEventAsync;

        event AsyncEventHandler<PollBeginEvent> OnPollBeginEventAsync;

        event AsyncEventHandler<PollProgressEvent> OnPollProgressEventAsync;

        event AsyncEventHandler<PollEndEvent> OnPollEndEventAsync;

        event AsyncEventHandler<PredictionBeginEvent> OnPredictionBeginEventAsync;

        event AsyncEventHandler<PredictionProgressEvent> OnPredictionProgressEventAsync;

        event AsyncEventHandler<PredictionLockEvent> OnPredictionLockEventAsync;

        event AsyncEventHandler<PredictionEndEvent> OnPredictionEndEventAsync;

        event AsyncEventHandler<CharityDonationEvent> OnCharityDonationEventAsync;

        event AsyncEventHandler<CharityCampaignStartEvent> OnCharityCampaignStartEventAsync;

        event AsyncEventHandler<CharityCampaignProgressEvent> OnCharityCampaignProgressEventAsync;

        event AsyncEventHandler<CharityCampaignStopEvent> OnCharityCampaignStopEventAsync;

        event AsyncEventHandler<GoalBeginEvent> OnGoalBeginEventAsync;

        event AsyncEventHandler<GoalProgressEvent> OnGoalProgressEventAsync;

        event AsyncEventHandler<GoalEndEvent> OnGoalEndEventAsync;

        event AsyncEventHandler<HypeTrainBeginEvent> OnHypeTrainBeginEventAsync;

        event AsyncEventHandler<HypeTrainProgressEvent> OnHypeTrainProgressEventAsync;

        event AsyncEventHandler<HypeTrainEndEvent> OnHypeTrainEndEventAsync;

        event AsyncEventHandler<ShieldModeBeginEvent> OnShieldModeBeginEventAsync;

        event AsyncEventHandler<ShieldModeEndEvent> OnShieldModeEndEventAsync;

        event AsyncEventHandler<ShoutoutCreateEvent> OnShoutoutCreateEventAsync;

        event AsyncEventHandler<ShoutoutReceivedEvent> OnShoutoutReceivedEventAsync;

        event AsyncEventHandler<StreamOnlineEvent> OnStreamOnlineEventAsync;

        event AsyncEventHandler<StreamOfflineEvent> OnStreamOfflineEventAsync;

        /// <summary>
        /// Initializes connection to event sub servers.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="userId"></param>
        /// <param name="accessToken"></param>
        /// <param name="listOfSubs"></param>
        /// <returns></returns>
        Task<bool> StartAsync(string clientId, string userId, string accessToken, List<SubscriptionType> listOfSubs);

        /// <summary>
        ///  Provides way to change clientId, accessToken or list of subs during run
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="userId"></param>
        /// <param name="accessToken"></param>
        /// <param name="listOfSubs"></param>
        /// <returns></returns>
        Task UpdateOnFlyAsync(string clientId, string userId, string accessToken, List<SubscriptionType> listOfSubs);

        /// <summary>
        /// Disconnects client from servers and cleans up subscriptions
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}
