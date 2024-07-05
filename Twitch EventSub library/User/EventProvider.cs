using Microsoft.Extensions.Logging;
using Twitch.EventSub.API.Extensions;
using Twitch.EventSub.API.Models;
using Twitch.EventSub.CoreFunctions;
using Twitch.EventSub.Interfaces;
using Twitch.EventSub.Messages.NotificationMessage.Events;

namespace Twitch.EventSub.User
{
    public class EventProvider : IEventProvider
    {
        private readonly UserSequencer _userSequencer;

        #region Available events
        public event AsyncEventHandler<UpdateNotificationEvent> OnUpdateNotificationEventAsync;
        public event AsyncEventHandler<FollowEvent> OnFollowEventAsync;
        public event AsyncEventHandler<ChannelChatMessage> OnChannelChatEventAsync;
        public event AsyncEventHandler<SubscribeEvent> OnSubscribeEventAsync;
        public event AsyncEventHandler<SubscribeEndEvent> OnSubscribeEndEventAsync;
        public event AsyncEventHandler<SubscriptionGiftEvent> OnSubscriptionGiftEventAsync;
        public event AsyncEventHandler<SubscriptionMessageEvent> OnSubscriptionMessageEventAsync;
        public event AsyncEventHandler<CheerEvent> OnCheerEventAsync;
        public event AsyncEventHandler<RaidEvent> OnRaidEventAsync;
        public event AsyncEventHandler<BanEvent> OnBanEventAsync;
        public event AsyncEventHandler<UnBanEvent> OnUnBanEventAsync;
        public event AsyncEventHandler<ModeratorAddEvent> OnModeratorAddEventAsync;
        public event AsyncEventHandler<ModeratorRemoveEvent> OnModeratorRemoveEventAsync;
        public event AsyncEventHandler<GuestStarSessionBeginEvent> OnGuestStarSessionBeginEventAsync;
        public event AsyncEventHandler<GuestStarSessionEndEvent> OnGuestStarSessionEndEventAsync;
        public event AsyncEventHandler<GuestStarGuestUpdateEvent> OnGuestStarGuestUpdateEventAsync;
        public event AsyncEventHandler<GuestStarSlotUpdateEvent> OnGuestStarSlotUpdateEventAsync;
        public event AsyncEventHandler<GuestStarSettingsUpdateEvent> OnGuestStarSettingsUpdateEventAsync;
        public event AsyncEventHandler<PointsCustomRewardAddEvent> OnPointsCustomRewardAddEventAsync;
        public event AsyncEventHandler<PointsCustomRewardUpdateEvent> OnPointsCustomRewardUpdateEventAsync;
        public event AsyncEventHandler<PointsCustomRewardRemoveEvent> OnPointsCustomRewardRemoveEventAsync;
        public event AsyncEventHandler<PointsCustomRewardRedemptionAddEvent> OnPointsCustomRewardRedemptionAddEventAsync;
        public event AsyncEventHandler<PointsCustomRewardRedemptionUpdateEvent> OnPointsCustomRewardRedemptionUpdateEventAsync;
        public event AsyncEventHandler<PollBeginEvent> OnPollBeginEventAsync;
        public event AsyncEventHandler<PollProgressEvent> OnPollProgressEventAsync;
        public event AsyncEventHandler<PollEndEvent> OnPollEndEventAsync;
        public event AsyncEventHandler<PredictionBeginEvent> OnPredictionBeginEventAsync;
        public event AsyncEventHandler<PredictionProgressEvent> OnPredictionProgressEventAsync;
        public event AsyncEventHandler<PredictionLockEvent> OnPredictionLockEventAsync;
        public event AsyncEventHandler<PredictionEndEvent> OnPredictionEndEventAsync;
        public event AsyncEventHandler<CharityDonationEvent> OnCharityDonationEventAsync;
        public event AsyncEventHandler<CharityCampaignStartEvent> OnCharityCampaignStartEventAsync;
        public event AsyncEventHandler<CharityCampaignProgressEvent> OnCharityCampaignProgressEventAsync;
        public event AsyncEventHandler<CharityCampaignStopEvent> OnCharityCampaignStopEventAsync;
        //public event AsyncEventHandler<DropEntitlementGrantEvent> OnDropEntitlementGrantEventAsync;
        //public event AsyncEventHandler<ExtensionBitsTransactionCreateEvent> OnExtensionBitsTransactionCreateEventAsync;
        public event AsyncEventHandler<GoalBeginEvent> OnGoalBeginEventAsync;
        public event AsyncEventHandler<GoalProgressEvent> OnGoalProgressEventAsync;
        public event AsyncEventHandler<GoalEndEvent> OnGoalEndEventAsync;
        public event AsyncEventHandler<HypeTrainBeginEvent> OnHypeTrainBeginEventAsync;
        public event AsyncEventHandler<HypeTrainProgressEvent> OnHypeTrainProgressEventAsync;
        public event AsyncEventHandler<HypeTrainEndEvent> OnHypeTrainEndEventAsync;
        public event AsyncEventHandler<ShieldModeBeginEvent> OnShieldModeBeginEventAsync;
        public event AsyncEventHandler<ShieldModeEndEvent> OnShieldModeEndEventAsync;
        public event AsyncEventHandler<ShoutoutCreateEvent> OnShoutoutCreateEventAsync;
        public event AsyncEventHandler<ShoutoutReceivedEvent> OnShoutoutReceivedEventAsync;
        public event AsyncEventHandler<StreamOnlineEvent> OnStreamOnlineEventAsync;
        public event AsyncEventHandler<StreamOfflineEvent> OnStreamOfflineEventAsync;
        //public event AsyncEventHandler<UserAuthorizationGrantEvent> OnUserAuthorizationGrantEventAsync;
        //public event AsyncEventHandler<UserAuthorizationRevokeEvent> OnUserAuthorizationRevokeEventAsync;
        //public event AsyncEventHandler<UserUpdateEvent> OnUserUpdateEventAsync;
        #endregion

        public event EventHandler<string?> OnUnexpectedConnectionTermination;
        public event AsyncEventHandler<InvalidAccessTokenException> OnRefreshTokenAsync;
        public event AsyncEventHandler<string?> OnRawMessageAsync;

        public EventProvider(
            string userId,
            string accessToken,
            List<SubscriptionType> listOfSubs,
            string clientId,
            ILogger logger)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId, nameof(userId));
            ArgumentException.ThrowIfNullOrWhiteSpace(accessToken, nameof(accessToken));
            ArgumentException.ThrowIfNullOrWhiteSpace(clientId, nameof(clientId));
            ArgumentNullException.ThrowIfNull(listOfSubs, nameof(listOfSubs));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            var _listOfSubs = new List<CreateSubscriptionRequest>();
            foreach (var type in listOfSubs)
            {
                _listOfSubs.Add(new CreateSubscriptionRequest()
                {
                    Transport = new Transport() { Method = "websocket" },
                    Condition = new Condition()
                }.SetSubscriptionType(type, userId));
            }
            _userSequencer = new UserSequencer(userId, accessToken, _listOfSubs, clientId, logger);

            _userSequencer.AccessTokenRequestedEvent += AccessTokenRequestedEventAsync;
            _userSequencer.OnRawMessageRecievedAsync += OnRawMessageRecievedAsync;
            _userSequencer.OnOutsideDisconnectAsync += OnOutsideDisconnectAsync;
            _userSequencer.OnNotificationMessageAsync += Sequencer_OnNotificationMessageAsync;
        }

        public Task StartAsync()
        {
            return _userSequencer.StartAsync();
        }

        public Task StopAsync()
        {
            return _userSequencer.StopAsync();
        }

        public bool Update(string accessToken, List<SubscriptionType> listOfSubs)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accessToken, nameof(accessToken));
            ArgumentNullException.ThrowIfNull(listOfSubs, nameof(listOfSubs));

            var _listOfSubs = new List<CreateSubscriptionRequest>();
            foreach (var type in listOfSubs)
            {
                _listOfSubs.Add(new CreateSubscriptionRequest()
                {
                    Transport = new Transport() { Method = "websocket" },
                    Condition = new Condition()
                }.SetSubscriptionType(type, _userSequencer.UserId));
            }

            return _userSequencer.Update(accessToken, _listOfSubs);
        }


        private Task OnOutsideDisconnectAsync(object sender, string? e)
        {
            OnUnexpectedConnectionTermination.Invoke(sender, e);
            return Task.CompletedTask;
        }


        private async Task OnRawMessageRecievedAsync(object sender, string? e)
        {
            await OnRawMessageAsync.TryInvoke(sender, e);
        }

        private async Task AccessTokenRequestedEventAsync(object sender, InvalidAccessTokenException e)
        {
            await OnRefreshTokenAsync.TryInvoke(sender, e);
        }


        private async Task Sequencer_OnNotificationMessageAsync(object sender, Messages.NotificationMessage.WebSocketNotificationPayload e)
        {
            switch (e.Event)
            {
                case UpdateNotificationEvent updateEvent:
                    await OnUpdateNotificationEventAsync.TryInvoke(sender, updateEvent);
                    break;

                case FollowEvent followEvent:
                    await OnFollowEventAsync.TryInvoke(sender, followEvent);
                    break;

                case SubscribeEndEvent subscribeEndEvent:
                    await OnSubscribeEndEventAsync.TryInvoke(sender, subscribeEndEvent);
                    break;

                case SubscribeEvent subscribeEvent:
                    await OnSubscribeEventAsync.TryInvoke(sender, subscribeEvent);
                    break;

                case ChannelChatMessage channelChatMessage:
                    await OnChannelChatEventAsync.TryInvoke(sender, channelChatMessage);
                    break;

                case SubscriptionGiftEvent subscriptionGiftEvent:
                    await OnSubscriptionGiftEventAsync.TryInvoke(sender, subscriptionGiftEvent);
                    break;

                case SubscriptionMessageEvent subscriptionMessageEvent:
                    await OnSubscriptionMessageEventAsync.TryInvoke(sender, subscriptionMessageEvent);
                    break;

                case CheerEvent cheerEvent:
                    await OnCheerEventAsync.TryInvoke(sender, cheerEvent);
                    break;

                case RaidEvent raidEvent:
                    await OnRaidEventAsync.TryInvoke(sender, raidEvent);
                    break;

                case BanEvent banEvent:
                    await OnBanEventAsync.TryInvoke(sender, banEvent);
                    break;

                case UnBanEvent unBanEvent:
                    await OnUnBanEventAsync.TryInvoke(sender, unBanEvent);
                    break;

                case ModeratorRemoveEvent moderatorRemoveEvent:
                    await OnModeratorRemoveEventAsync.TryInvoke(sender, moderatorRemoveEvent);
                    break;

                case ModeratorAddEvent moderatorAddEvent:
                    await OnModeratorAddEventAsync.TryInvoke(sender, moderatorAddEvent);
                    break;

                case GuestStarSessionEndEvent guestStarSessionEndEvent:
                    await OnGuestStarSessionEndEventAsync.TryInvoke(sender, guestStarSessionEndEvent);
                    break;

                case GuestStarSessionBeginEvent guestStarSessionBeginEvent:
                    await OnGuestStarSessionBeginEventAsync.TryInvoke(sender, guestStarSessionBeginEvent);
                    break;


                case GuestStarGuestUpdateEvent guestStarGuestUpdateEvent:
                    await OnGuestStarGuestUpdateEventAsync.TryInvoke(sender, guestStarGuestUpdateEvent);
                    break;

                case GuestStarSlotUpdateEvent guestStarSlotUpdateEvent:
                    await OnGuestStarSlotUpdateEventAsync.TryInvoke(sender, guestStarSlotUpdateEvent);
                    break;

                case GuestStarSettingsUpdateEvent guestStarSettingsUpdateEvent:
                    await OnGuestStarSettingsUpdateEventAsync.TryInvoke(sender, guestStarSettingsUpdateEvent);
                    break;

                case PointsCustomRewardUpdateEvent customRewardUpdateEvent:
                    await OnPointsCustomRewardUpdateEventAsync.TryInvoke(sender, customRewardUpdateEvent);
                    break;

                case PointsCustomRewardRemoveEvent customRewardRemoveEvent:
                    await OnPointsCustomRewardRemoveEventAsync.TryInvoke(sender, customRewardRemoveEvent);
                    break;

                case PointsCustomRewardRedemptionUpdateEvent customRewardRedemptionUpdateEvent:
                    await OnPointsCustomRewardRedemptionUpdateEventAsync.TryInvoke(sender, customRewardRedemptionUpdateEvent);
                    break;

                case PointsCustomRewardRedemptionAddEvent customRewardRedemptionAddEvent:
                    await OnPointsCustomRewardRedemptionAddEventAsync.TryInvoke(sender, customRewardRedemptionAddEvent);
                    break;

                case PointsCustomRewardAddEvent customRewardAddEvent:
                    await OnPointsCustomRewardAddEventAsync.TryInvoke(sender, customRewardAddEvent);
                    break;

                case PollProgressEvent pollProgressEvent:
                    await OnPollProgressEventAsync.TryInvoke(sender, pollProgressEvent);
                    break;

                case PollEndEvent pollEndEvent:
                    await OnPollEndEventAsync.TryInvoke(sender, pollEndEvent);
                    break;

                case PollBeginEvent pollBeginEvent:
                    await OnPollBeginEventAsync.TryInvoke(sender, pollBeginEvent);
                    break;

                case PredictionProgressEvent predictionProgressEvent:
                    await OnPredictionProgressEventAsync.TryInvoke(sender, predictionProgressEvent);
                    break;

                case PredictionLockEvent predictionLockEvent:
                    await OnPredictionLockEventAsync.TryInvoke(sender, predictionLockEvent);
                    break;

                case PredictionEndEvent predictionEndEvent:
                    await OnPredictionEndEventAsync.TryInvoke(sender, predictionEndEvent);
                    break;

                case PredictionBeginEvent predictionBeginEvent:
                    await OnPredictionBeginEventAsync.TryInvoke(sender, predictionBeginEvent);
                    break;

                case HypeTrainProgressEvent hypeTrainProgressEvent:
                    await OnHypeTrainProgressEventAsync.TryInvoke(sender, hypeTrainProgressEvent);
                    break;

                case HypeTrainEndEvent hypeTrainEndEvent:
                    await OnHypeTrainEndEventAsync.TryInvoke(sender, hypeTrainEndEvent);
                    break;

                case HypeTrainBeginEvent hypeTrainBeginEvent:
                    await OnHypeTrainBeginEventAsync.TryInvoke(sender, hypeTrainBeginEvent);
                    break;

                case CharityCampaignStartEvent charityCampaignStartEvent:
                    await OnCharityCampaignStartEventAsync.TryInvoke(sender, charityCampaignStartEvent);
                    break;

                case CharityCampaignProgressEvent charityCampaignProgressEvent:
                    await OnCharityCampaignProgressEventAsync.TryInvoke(sender, charityCampaignProgressEvent);
                    break;

                case CharityCampaignStopEvent charityCampaignStopEvent:
                    await OnCharityCampaignStopEventAsync.TryInvoke(sender, charityCampaignStopEvent);
                    break;

                case CharityDonationEvent charityDonationEvent:
                    await OnCharityDonationEventAsync.TryInvoke(sender, charityDonationEvent);
                    break;

                //Cant be used by websocket
                //case DropEntitlementGrantEvent dropEntitlementGrantEvent:
                //    await OnDropEntitlementGrantEventAsync.TryInvoke(sender, dropEntitlementGrantEvent);
                //    break;

                //case ExtensionBitsTransactionCreateEvent bitsTransactionCreateEvent:
                //    await OnExtensionBitsTransactionCreateEventAsync.TryInvoke(sender, bitsTransactionCreateEvent);
                //    break;


                case GoalProgressEvent goalProgressEvent:
                    await OnGoalProgressEventAsync.TryInvoke(sender, goalProgressEvent);
                    break;

                case GoalEndEvent goalEndEvent:
                    await OnGoalEndEventAsync.TryInvoke(sender, goalEndEvent);
                    break;

                case GoalBeginEvent goalBeginEvent:
                    await OnGoalBeginEventAsync.TryInvoke(sender, goalBeginEvent);
                    break;

                case ShieldModeEndEvent shieldModeEndEvent:
                    await OnShieldModeEndEventAsync.TryInvoke(sender, shieldModeEndEvent);
                    break;

                case ShieldModeBeginEvent shieldModeBeginEvent:
                    await OnShieldModeBeginEventAsync.TryInvoke(sender, shieldModeBeginEvent);
                    break;

                case ShoutoutCreateEvent shoutoutCreateEvent:
                    await OnShoutoutCreateEventAsync.TryInvoke(sender, shoutoutCreateEvent);
                    break;

                case ShoutoutReceivedEvent shoutoutReceivedEvent:
                    await OnShoutoutReceivedEventAsync.TryInvoke(sender, shoutoutReceivedEvent);
                    break;

                case StreamOnlineEvent streamOnlineEvent:
                    await OnStreamOnlineEventAsync.TryInvoke(sender, streamOnlineEvent);
                    break;

                case StreamOfflineEvent streamOfflineEvent:
                    await OnStreamOfflineEventAsync.TryInvoke(sender, streamOfflineEvent);
                    break;
                //Cant be used by websocket

                //case UserAuthorizationGrantEvent userAuthorizationGrantEvent:
                //    await OnUserAuthorizationGrantEventAsync.TryInvoke(sender, userAuthorizationGrantEvent);
                //    break;

                //case UserAuthorizationRevokeEvent userAuthorizationRevokeEvent:
                //    await OnUserAuthorizationRevokeEventAsync.TryInvoke(sender, userAuthorizationRevokeEvent);
                //    break;

                //User update doesnt maintain proper structure. 
                //NOT SUPPORTED

                //case UserUpdateEvent userUpdateEvent:
                //    await OnUserUpdateEventAsync.TryInvoke(sender, userUpdateEvent);
                //    break;


                default:
                    throw new NotImplementedException();
            }
        }
    }
}
