using Microsoft.Extensions.Logging;
using Twitch.EventSub.API.Extensions;
using Twitch.EventSub.API.Models;
using Twitch.EventSub.CoreFunctions;
using Twitch.EventSub.Interfaces;
using Twitch.EventSub.Library.CoreFunctions;
using Twitch.EventSub.Library.Messages.NotificationMessage.Events;
using Twitch.EventSub.Messages.NotificationMessage.Events;

namespace Twitch.EventSub
{
    public class EventSubClient : IEventSubClient
    {
        private readonly ILogger _logger;
        private readonly EventSubscriptionManager _manager;
        private readonly EventSubSocketWrapper _socket;
        private string _clientId;
        private string _accessToken;
        private List<CreateSubscriptionRequest> _listOfSubs;
        public event EventHandler<string?> OnUnexpectedConnectionTermination;
        public event AsyncEventHandler<InvalidAccessTokenException> OnRefreshTokenAsync;
        public event AsyncEventHandler<string?> OnRawMessageAsync;
        public bool IsConnected { get; private set; }
        public EventSubClient(ILogger<EventSubClient> logger, EventSubClientOptions options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (options.CommunicationSpeed > TimeSpan.FromSeconds(10))
            {
                _logger.LogWarning("[EventSubClient] Communication MUST be faster or equal 10 s");
                return;
            }
            _manager = new EventSubscriptionManager(logger);
            _socket = new EventSubSocketWrapper(logger);
            _socket.OnRawMessageRecievedAsync += SocketOnRawMessageRecievedAsync;
            _socket.OnNotificationMessageAsync += SocketOnNotificationAsync;
            _socket.OnRegisterSubscriptionsAsync += SocketOnRegisterSubscriptionsAsyncAsync;
            _socket.OnRevocationMessageAsync += SocketOnRevocationMessageAsyncAsync;
            _socket.OnOutsideDisconnectAsync += SocketOnOutsideDisconnectAsyncAsync;
            _manager.OnRefreshTokenRequestAsync += ManagerOnRefreshTokenRequestAsyncAsync;
            IsConnected = false;
        }

        private async Task SocketOnRawMessageRecievedAsync(object sender, string? e)
        {
           await OnRawMessageAsync.TryInvoke(sender, e);
        }

        public EventSubClient(ILogger<EventSubClient> logger) : this(logger, new EventSubClientOptions())
        {
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

        private async Task ManagerOnRefreshTokenRequestAsyncAsync(object sender, InvalidAccessTokenException e)
        {
            //I know, this is suboptimal you can subscribe ManagerOnRefreshTokenRequest without further skip
            await OnRefreshTokenAsync.TryInvoke(this, e);
        }

        /// <summary>
        ///  Wrapper got into unexpected connection termination. Triggers Manager to clean up and stop repeating checks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SocketOnOutsideDisconnectAsyncAsync(object sender, string? e)
        {
            IsConnected = false;
            OnUnexpectedConnectionTermination.Invoke(sender, e);
            await _manager.StopAsync();
        }
        /// <summary>
        /// Revocation messages will probably pile up due big number of requests at same time
        /// Giving it here some time to settle and then run checks 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SocketOnRevocationMessageAsyncAsync(object sender, Messages.RevocationMessage.WebSocketRevocationMessage e)
        {
            await _manager.RevocationResolverAsync(e);
        }
        /// <summary>
        /// Initializes connection to event sub servers.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="userId"></param>
        /// <param name="accessToken"></param>
        /// <param name="listOfSubs"></param>
        /// <returns></returns>
        public async Task<bool> StartAsync(string clientId, string userId, string accessToken, List<SubscriptionType> listOfSubs)
        {
            StartUp(clientId, userId, accessToken, listOfSubs);
            if (await _socket.ConnectAsync())
            {
                IsConnected = true;
                return true;
            }
            _logger.LogInformation("[EventSubClient] Connection unsuccessful");
            return false;
        }
        /// <summary>
        ///  Provides way to change clientId, accessToken or list of subs during run
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="userId"></param>
        /// <param name="accessToken"></param>
        /// <param name="listOfSubs"></param>
        /// <returns></returns>
        public async Task UpdateOnFlyAsync(string clientId, string userId, string accessToken, List<SubscriptionType> listOfSubs)
        {
            StartUp(clientId, userId, accessToken, listOfSubs);
            await _manager.UpdateOnFlyAsync(clientId, accessToken, _listOfSubs);
        }
        /// <summary>
        /// Provides Initialization part of function. Links all requested subscriptions to proper requests
        /// SetSubscriptionType may also support selective reward subscriptions. We are currently supporting only all reward sub.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="userId"></param>
        /// <param name="accessToken"></param>
        /// <param name="listOfSubs"></param>
        private void StartUp(string clientId, string userId, string accessToken, List<SubscriptionType> listOfSubs)
        {
            _clientId = clientId;
            _accessToken = accessToken;
            _listOfSubs = new List<CreateSubscriptionRequest>();
            foreach (var type in listOfSubs)
            {
                _listOfSubs.Add(new CreateSubscriptionRequest()
                {
                    Transport = new Transport() { Method = "websocket" },
                    Condition = new Condition()
                }.SetSubscriptionType(type, userId));

            }
        }
        /// <summary>
        /// Disconnects client from servers and cleans up subscriptions
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            IsConnected = false;
            await _socket.DisconnectAsync();
            await _manager.StopAsync();
        }
        /// <summary>
        /// This event is triggered early in communication and provides session id, which is critical for proper function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        private async Task SocketOnRegisterSubscriptionsAsyncAsync(object sender, string? sessionId)
        {
            await _manager.SetupAsync(_clientId, _accessToken, sessionId, _listOfSubs);
            _manager.Start();
        }
        /// <summary>
        /// Provides all notifications to user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task SocketOnNotificationAsync(object sender, Messages.NotificationMessage.WebSocketNotificationPayload e)
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
