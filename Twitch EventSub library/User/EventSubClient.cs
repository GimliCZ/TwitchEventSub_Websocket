using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Twitch.EventSub.API.Extensions;
using Twitch.EventSub.API.Models;
using Twitch.EventSub.CoreFunctions;
using Twitch.EventSub.Interfaces;
using Twitch.EventSub.Messages.NotificationMessage.Events;

namespace Twitch.EventSub.User
{
    public class EventSubClient : IEventSubClient
    {
        private readonly ILogger _logger;
        private string _clientId;
        private ConcurrentDictionary<string, UserSequencer> _userDictionary;

        public event EventHandler<string?> OnUnexpectedConnectionTermination;
        public event AsyncEventHandler<InvalidAccessTokenException, UserSequencer> OnRefreshTokenAsync;
        public event AsyncEventHandler<string?, UserSequencer> OnRawMessageAsync;

        #region Available events

        public event AsyncEventHandler<UpdateNotificationEvent, UserSequencer> OnUpdateNotificationEventAsync;
        public event AsyncEventHandler<FollowEvent, UserSequencer> OnFollowEventAsync;
        public event AsyncEventHandler<ChannelChatMessage, UserSequencer> OnChannelChatEventAsync;
        public event AsyncEventHandler<SubscribeEvent, UserSequencer> OnSubscribeEventAsync;
        public event AsyncEventHandler<SubscribeEndEvent, UserSequencer> OnSubscribeEndEventAsync;
        public event AsyncEventHandler<SubscriptionGiftEvent, UserSequencer> OnSubscriptionGiftEventAsync;
        public event AsyncEventHandler<SubscriptionMessageEvent, UserSequencer> OnSubscriptionMessageEventAsync;
        public event AsyncEventHandler<CheerEvent, UserSequencer> OnCheerEventAsync;
        public event AsyncEventHandler<RaidEvent, UserSequencer> OnRaidEventAsync;
        public event AsyncEventHandler<BanEvent, UserSequencer> OnBanEventAsync;
        public event AsyncEventHandler<UnBanEvent, UserSequencer> OnUnBanEventAsync;
        public event AsyncEventHandler<ModeratorAddEvent, UserSequencer> OnModeratorAddEventAsync;
        public event AsyncEventHandler<ModeratorRemoveEvent, UserSequencer> OnModeratorRemoveEventAsync;
        public event AsyncEventHandler<GuestStarSessionBeginEvent, UserSequencer> OnGuestStarSessionBeginEventAsync;
        public event AsyncEventHandler<GuestStarSessionEndEvent, UserSequencer> OnGuestStarSessionEndEventAsync;
        public event AsyncEventHandler<GuestStarGuestUpdateEvent, UserSequencer> OnGuestStarGuestUpdateEventAsync;
        public event AsyncEventHandler<GuestStarSlotUpdateEvent, UserSequencer> OnGuestStarSlotUpdateEventAsync;
        public event AsyncEventHandler<GuestStarSettingsUpdateEvent, UserSequencer> OnGuestStarSettingsUpdateEventAsync;
        public event AsyncEventHandler<PointsCustomRewardAddEvent, UserSequencer> OnPointsCustomRewardAddEventAsync;
        public event AsyncEventHandler<PointsCustomRewardUpdateEvent, UserSequencer> OnPointsCustomRewardUpdateEventAsync;
        public event AsyncEventHandler<PointsCustomRewardRemoveEvent, UserSequencer> OnPointsCustomRewardRemoveEventAsync;
        public event AsyncEventHandler<PointsCustomRewardRedemptionAddEvent, UserSequencer> OnPointsCustomRewardRedemptionAddEventAsync;
        public event AsyncEventHandler<PointsCustomRewardRedemptionUpdateEvent, UserSequencer> OnPointsCustomRewardRedemptionUpdateEventAsync;
        public event AsyncEventHandler<PollBeginEvent, UserSequencer> OnPollBeginEventAsync;
        public event AsyncEventHandler<PollProgressEvent, UserSequencer> OnPollProgressEventAsync;
        public event AsyncEventHandler<PollEndEvent, UserSequencer> OnPollEndEventAsync;
        public event AsyncEventHandler<PredictionBeginEvent, UserSequencer> OnPredictionBeginEventAsync;
        public event AsyncEventHandler<PredictionProgressEvent, UserSequencer> OnPredictionProgressEventAsync;
        public event AsyncEventHandler<PredictionLockEvent, UserSequencer> OnPredictionLockEventAsync;
        public event AsyncEventHandler<PredictionEndEvent, UserSequencer> OnPredictionEndEventAsync;
        public event AsyncEventHandler<CharityDonationEvent, UserSequencer> OnCharityDonationEventAsync;
        public event AsyncEventHandler<CharityCampaignStartEvent, UserSequencer> OnCharityCampaignStartEventAsync;
        public event AsyncEventHandler<CharityCampaignProgressEvent, UserSequencer> OnCharityCampaignProgressEventAsync;
        public event AsyncEventHandler<CharityCampaignStopEvent, UserSequencer> OnCharityCampaignStopEventAsync;
        //public event AsyncEventHandler<DropEntitlementGrantEvent> OnDropEntitlementGrantEventAsync;
        //public event AsyncEventHandler<ExtensionBitsTransactionCreateEvent> OnExtensionBitsTransactionCreateEventAsync;
        public event AsyncEventHandler<GoalBeginEvent, UserSequencer> OnGoalBeginEventAsync;
        public event AsyncEventHandler<GoalProgressEvent, UserSequencer> OnGoalProgressEventAsync;
        public event AsyncEventHandler<GoalEndEvent, UserSequencer> OnGoalEndEventAsync;
        public event AsyncEventHandler<HypeTrainBeginEvent, UserSequencer> OnHypeTrainBeginEventAsync;
        public event AsyncEventHandler<HypeTrainProgressEvent, UserSequencer> OnHypeTrainProgressEventAsync;
        public event AsyncEventHandler<HypeTrainEndEvent, UserSequencer> OnHypeTrainEndEventAsync;
        public event AsyncEventHandler<ShieldModeBeginEvent, UserSequencer> OnShieldModeBeginEventAsync;
        public event AsyncEventHandler<ShieldModeEndEvent, UserSequencer> OnShieldModeEndEventAsync;
        public event AsyncEventHandler<ShoutoutCreateEvent, UserSequencer> OnShoutoutCreateEventAsync;
        public event AsyncEventHandler<ShoutoutReceivedEvent, UserSequencer> OnShoutoutReceivedEventAsync;
        public event AsyncEventHandler<StreamOnlineEvent, UserSequencer> OnStreamOnlineEventAsync;
        public event AsyncEventHandler<StreamOfflineEvent, UserSequencer> OnStreamOfflineEventAsync;
        //public event AsyncEventHandler<UserAuthorizationGrantEvent> OnUserAuthorizationGrantEventAsync;
        //public event AsyncEventHandler<UserAuthorizationRevokeEvent> OnUserAuthorizationRevokeEventAsync;
        //public event AsyncEventHandler<UserUpdateEvent> OnUserUpdateEventAsync;

        #endregion

        public EventSubClient(string clientId, ILogger<EventSubClient> logger)
        {
            _clientId = clientId;
            _userDictionary = new ConcurrentDictionary<string, UserSequencer>();
            _logger = logger;
        }
        public async Task<bool> AddUserAsync(string userId, string accessToken, List<SubscriptionType> listOfSubs)
        {
            var _listOfSubs = new List<CreateSubscriptionRequest>();
            foreach (var type in listOfSubs)
            {
                _listOfSubs.Add(new CreateSubscriptionRequest()
                {
                    Transport = new Transport() { Method = "websocket" },
                    Condition = new Condition()
                }.SetSubscriptionType(type, userId));
            }
            var sequencer = new UserSequencer(userId, accessToken, _listOfSubs, _clientId, _logger);

            sequencer.AccessTokenRequestedEvent += Sequencer_AccessTokenRequestedEvent;
            sequencer.OnRawMessageRecievedAsync += Sequencer_OnRawMessageRecievedAsync;
            sequencer.OnNotificationMessageAsync += Sequencer_OnNotificationMessageAsync;
            sequencer.OnOutsideDisconnectAsync += Sequencer_OnOutsideDisconnectAsync;

            if (_userDictionary.TryAdd(userId, sequencer))
            {
                await _userDictionary[userId].StartAsync();
                return true;
            }
            return false;
        }

        private Task Sequencer_OnOutsideDisconnectAsync(UserSequencer sender, string? e)
        {
            OnUnexpectedConnectionTermination.Invoke(sender, e);
            return Task.CompletedTask;
        }

        private async Task Sequencer_OnNotificationMessageAsync(UserSequencer sender, Messages.NotificationMessage.WebSocketNotificationPayload e)
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

        private async Task Sequencer_OnRawMessageRecievedAsync(UserSequencer sender, string? e)
        {
            await OnRawMessageAsync.TryInvoke(sender, e);
        }

        private async Task Sequencer_AccessTokenRequestedEvent(UserSequencer sender, InvalidAccessTokenException e)
        {
            await OnRefreshTokenAsync.TryInvoke(sender, e);
        }

        public bool UpdateUser(string userId, string accessToken, List<SubscriptionType> listOfSubs)
        {
            var _listOfSubs = new List<CreateSubscriptionRequest>();
            foreach (var type in listOfSubs)
            {
                _listOfSubs.Add(new CreateSubscriptionRequest()
                {
                    Transport = new Transport() { Method = "websocket" },
                    Condition = new Condition()
                }.SetSubscriptionType(type, userId));
            }

            if (_userDictionary.TryGetValue(userId, out var sequencerold))
            {
                return sequencerold.Update(userId, accessToken, _listOfSubs);
            }
            else
            {
                return false;
            }
        }
        public async Task<bool> DeleteUserAsync(string userId)
        {
            if (_userDictionary.TryGetValue(userId, out var sequencer))
            {
                await sequencer.StopAsync();
                if (_userDictionary.TryRemove(userId, out _))
                {
                    return true;
                }
            }
            return false;
        }
    }
}