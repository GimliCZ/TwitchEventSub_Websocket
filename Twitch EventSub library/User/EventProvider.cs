using Microsoft.Extensions.Logging;
using Twitch.EventSub.API.Extensions;
using Twitch.EventSub.API.Models;
using Twitch.EventSub.CoreFunctions;
using Twitch.EventSub.Interfaces;
using Twitch.EventSub.Messages.NotificationMessage.Events;

namespace Twitch.EventSub.User
{
    /// <summary>
    /// Primary source of events
    /// OnRefreshTokenAsync event is mandatory for propper function
    /// </summary>
    public class EventProvider : IEventProvider
    {
        private string _accessToken;
        private string _clientId;
        private List<SubscriptionType> _listOfSubs;
        private ILogger _logger;
        private string _userId;
        private UserSequencer _userSequencer;

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
            _userId = userId;
            _accessToken = accessToken;
            _listOfSubs = listOfSubs;
            _clientId = clientId;
            _logger = logger;

            Create();

        }

        /// <summary>
        /// Directly reports Connection state from Socket, may be used for reconnect detection
        /// </summary>
        public bool IsConnected => _userSequencer?.Socket?.IsRunning == true;

        /// <summary>
        /// Notifies about connection termination.
        /// May contain also internal disconnect, so take with grain of salt
        /// </summary>
        public event EventHandler<string?> OnUnexpectedConnectionTermination;

        /// <summary>
        /// Mandatory event for refreshing Access Token. To Update token use Update procedure of client
        /// </summary>
        public event AsyncEventHandler<InvalidAccessTokenException> OnRefreshTokenAsync;

        /// <summary>
        /// Raw messages
        /// </summary>
        public event AsyncEventHandler<string?> OnRawMessageAsync;

        /// <summary>
        /// Method to create UserSequencer instance and set up event handlers
        /// </summary>
        private void Create()
        {
            var listOfRequests = new List<CreateSubscriptionRequest>();
            foreach (var type in _listOfSubs)
            {
                listOfRequests.Add(new CreateSubscriptionRequest()
                {
                    Transport = new Transport() { Method = "websocket" },
                    Condition = new Condition()
                }.SetSubscriptionType(type, _userId));
            }
            _userSequencer = new UserSequencer(_userId, _accessToken, listOfRequests, _clientId, _logger);

            _userSequencer.AccessTokenRequestedEvent += AccessTokenRequestedEventAsync;
            _userSequencer.OnRawMessageRecievedAsync += OnRawMessageReceivedAsync;
            _userSequencer.OnOutsideDisconnectAsync += OnOutsideDisconnectAsync;
            _userSequencer.OnNotificationMessageAsync += Sequencer_OnNotificationMessageAsync;
            _userSequencer.OnDispose += _userSequencer_OnDispose;
        }

        private void _userSequencer_OnDispose(object? sender, string? e)
        {
            _userSequencer.AccessTokenRequestedEvent -= AccessTokenRequestedEventAsync;
            _userSequencer.OnRawMessageRecievedAsync -= OnRawMessageReceivedAsync;
            _userSequencer.OnOutsideDisconnectAsync -= OnOutsideDisconnectAsync;
            _userSequencer.OnNotificationMessageAsync -= Sequencer_OnNotificationMessageAsync;
            _userSequencer.OnDispose -= _userSequencer_OnDispose;
        }

        /// <summary>
        /// Method to start the UserSequencer instance asynchronously
        /// Regenerates Sequencer object, in case if its internaly disposed
        /// </summary>
        /// <returns></returns>
        internal Task StartAsync()
        {
            if (_userSequencer.IsDisposed())
            {
                Create();
            }
            return _userSequencer.StartAsync();
        }

        /// <summary>
        /// Method to stop the UserSequencer instance asynchronously
        /// </summary>
        /// <returns>Returns true on success, false if object is in invalid state to be stopped.</returns>
        internal Task<bool> StopAsync()
        {
            return _userSequencer.StopAsync();
        }

        /// <summary>
        /// Updates parameters of userSequencer
        /// Primary usage is to update Access Token. 
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="listOfSubs"></param>
        /// <returns>Retuns true if update is successful</returns>
        internal bool Update(string accessToken, List<SubscriptionType> listOfSubs)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(accessToken, nameof(accessToken));
            ArgumentNullException.ThrowIfNull(listOfSubs, nameof(listOfSubs));

            _accessToken = accessToken;
            _listOfSubs = listOfSubs;

            var listOfRequests = new List<CreateSubscriptionRequest>();
            foreach (var type in listOfSubs)
            {
                listOfRequests.Add(new CreateSubscriptionRequest()
                {
                    Transport = new Transport() { Method = "websocket" },
                    Condition = new Condition()
                }.SetSubscriptionType(type, _userSequencer.UserId));
            }

            return _userSequencer.Update(accessToken, listOfRequests);
        }

        /// <summary>
        /// Notification about outside disconnect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnOutsideDisconnectAsync(object sender, string? e)
        {
            OnUnexpectedConnectionTermination.Invoke(sender, e);
            return;
        }

        /// <summary>
        /// Raw message handler.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task OnRawMessageReceivedAsync(object message, string? e)
        {
            await OnRawMessageAsync.TryInvoke(message, e);
        }

        /// <summary>
        /// Access Token request handler.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private async Task AccessTokenRequestedEventAsync(object sender, InvalidAccessTokenException ex)
        {
            await OnRefreshTokenAsync.TryInvoke(sender, ex);
        }

        /// <summary>
        /// Notification message handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
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
    }
}
