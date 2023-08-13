using Microsoft.Extensions.Logging;
using Twitch.EventSub.API.Models;
using Twitch.EventSub.CoreFunctions;
using Twitch.EventSub.API.Extensions;
using Twitch.EventSub.Messages.NotificationMessage.Events;
using Twitch.EventSub.Library.CoreFunctions;
using Twitch.EventSub.Library.Messages.NotificationMessage.Events;

namespace Twitch.EventSub
{
    public class EventSubClient
    {
        private readonly ILogger _logger;
        private readonly EventSubscriptionManager _manager;
        private readonly EventSubSocketWrapper _socket;
        private string _clientId;
        private string _accessToken;
        private List<CreateSubscriptionRequest> _listOfSubs;
        private readonly TimeSpan _communicationSpeed = TimeSpan.FromMilliseconds(300);

        public event EventHandler<string?> OnUnexpectedConnectionTermination;
        public event AsyncEventHandler<InvalidAccessTokenException> OnRefreshToken;
        public EventSubClient(ILogger Logger)
        {
            _logger = Logger;

            _manager = new EventSubscriptionManager(Logger, Logger);
            _socket = new EventSubSocketWrapper(Logger, Logger, Logger, _communicationSpeed);
            _socket.OnNotificationMessage += SocketOnNotification;
            _socket.OnRegisterSubscriptions += SocketOnRegisterSubscriptions;
            _socket.OnRevocationMessage += SocketOnRevocationMessage;
            _socket.OnOutsideDisconnect += SocketOnOutsideDisconnect;
            _manager.OnRefreshTokenRequest += ManagerOnRefreshTokenRequest;
        }

        #region Available events

        public event AsyncEventHandler<UpdateNotificationEvent> OnUpdateNotificationEvent;
        public event AsyncEventHandler<FollowEvent> OnFollowEvent;
        public event AsyncEventHandler<SubscribeEvent> OnSubscribeEvent;
        public event AsyncEventHandler<SubscribeEndEvent> OnSubscribeEndEvent;
        public event AsyncEventHandler<SubscriptionGiftEvent> OnSubscriptionGiftEvent;
        public event AsyncEventHandler<SubscriptionMessageEvent> OnSubscriptionMessageEvent;
        public event AsyncEventHandler<CheerEvent> OnCheerEvent;
        public event AsyncEventHandler<RaidEvent> OnRaidEvent;
        public event AsyncEventHandler<BanEvent> OnBanEvent;
        public event AsyncEventHandler<UnBanEvent> OnUnBanEvent;
        public event AsyncEventHandler<ModeratorAddEvent> OnModeratorAddEvent;
        public event AsyncEventHandler<ModeratorRemoveEvent> OnModeratorRemoveEvent;
        public event AsyncEventHandler<GuestStarSessionBeginEvent> OnGuestStarSessionBeginEvent;
        public event AsyncEventHandler<GuestStarSessionEndEvent> OnGuestStarSessionEndEvent;
        public event AsyncEventHandler<GuestStarGuestUpdateEvent> OnGuestStarGuestUpdateEvent;
        public event AsyncEventHandler<GuestStarSlotUpdateEvent> OnGuestStarSlotUpdateEvent;
        public event AsyncEventHandler<GuestStarSettingsUpdateEvent> OnGuestStarSettingsUpdateEvent;
        public event AsyncEventHandler<PointsCustomRewardAddEvent> OnPointsCustomRewardAddEvent;
        public event AsyncEventHandler<PointsCustomRewardUpdateEvent> OnPointsCustomRewardUpdateEvent;
        public event AsyncEventHandler<PointsCustomRewardRemoveEvent> OnPointsCustomRewardRemoveEvent;
        public event AsyncEventHandler<PointsCustomRewardRedemptionAddEvent> OnPointsCustomRewardRedemptionAddEvent;
        public event AsyncEventHandler<PointsCustomRewardRedemptionUpdateEvent> OnPointsCustomRewardRedemptionUpdateEvent;
        public event AsyncEventHandler<PollBeginEvent> OnPollBeginEvent;
        public event AsyncEventHandler<PollProgressEvent> OnPollProgressEvent;
        public event AsyncEventHandler<PollEndEvent> OnPollEndEvent;
        public event AsyncEventHandler<PredictionBeginEvent> OnPredictionBeginEvent;
        public event AsyncEventHandler<PredictionProgressEvent> OnPredictionProgressEvent;
        public event AsyncEventHandler<PredictionLockEvent> OnPredictionLockEvent;
        public event AsyncEventHandler<PredictionEndEvent> OnPredictionEndEvent;
        public event AsyncEventHandler<CharityDonationEvent> OnCharityDonationEvent;
        public event AsyncEventHandler<CharityCampaignStartEvent> OnCharityCampaignStartEvent;
        public event AsyncEventHandler<CharityCampaignProgressEvent> OnCharityCampaignProgressEvent;
        public event AsyncEventHandler<CharityCampaignStopEvent> OnCharityCampaignStopEvent;
        //public event AsyncEventHandler<DropEntitlementGrantEvent> OnDropEntitlementGrantEvent;
        //public event AsyncEventHandler<ExtensionBitsTransactionCreateEvent> OnExtensionBitsTransactionCreateEvent;
        public event AsyncEventHandler<GoalBeginEvent> OnGoalBeginEvent;
        public event AsyncEventHandler<GoalProgressEvent> OnGoalProgressEvent;
        public event AsyncEventHandler<GoalEndEvent> OnGoalEndEvent;
        public event AsyncEventHandler<HypeTrainBeginEvent> OnHypeTrainBeginEvent;
        public event AsyncEventHandler<HypeTrainProgressEvent> OnHypeTrainProgressEvent;
        public event AsyncEventHandler<HypeTrainEndEvent> OnHypeTrainEndEvent;
        public event AsyncEventHandler<ShieldModeBeginEvent> OnShieldModeBeginEvent;
        public event AsyncEventHandler<ShieldModeEndEvent> OnShieldModeEndEvent;
        public event AsyncEventHandler<ShoutoutCreateEvent> OnShoutoutCreateEvent;
        public event AsyncEventHandler<ShoutoutReceivedEvent> OnShoutoutReceivedEvent;
        public event AsyncEventHandler<StreamOnlineEvent> OnStreamOnlineEvent;
        public event AsyncEventHandler<StreamOfflineEvent> OnStreamOfflineEvent;
        //public event AsyncEventHandler<UserAuthorizationGrantEvent> OnUserAuthorizationGrantEvent;
        //public event AsyncEventHandler<UserAuthorizationRevokeEvent> OnUserAuthorizationRevokeEvent;
        //public event AsyncEventHandler<UserUpdateEvent> OnUserUpdateEvent;

        #endregion

        private async Task ManagerOnRefreshTokenRequest(object sender, InvalidAccessTokenException e)
        {
            //I know, this is suboptimal you can subscribe ManagerOnRefreshTokenRequest without further skip
            await OnRefreshToken.TryInvoke(this, e);
        }

        /// <summary>
        ///  Wrapper got into unexpected connection termination. Triggers Manager to clean up and stop repeating checks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SocketOnOutsideDisconnect(object sender, string? e)
        {
            OnUnexpectedConnectionTermination.Invoke(sender, e);
            await _manager.Stop();
        }
        /// <summary>
        /// Revocation messages will probably pile up due big number of requests at same time
        /// Giving it here some time to settle and then run checks 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SocketOnRevocationMessage(object sender, Messages.RevocationMessage.WebSocketRevocationMessage e)
        {
            await _manager.RevocationResolver(e);
        }
        /// <summary>
        /// Initializes connection to event sub servers.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="accessToken"></param>
        /// <param name="listOfSubs"></param>
        /// <returns></returns>
        public async Task<bool> Start(string clientId, string userId, string accessToken, List<SubscriptionType> listOfSubs)
        {
            StartUp(clientId, userId, accessToken, listOfSubs);
            if (await _socket.ConnectAsync())
            {
                return true;
            }
            _logger.LogInformation("Connection unsuccessful");
            return false;
        }
        /// <summary>
        /// Provides way to change clientId, accessToken or list of subs during run
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="accessToken"></param>
        /// <param name="listOfSubs"></param>
        /// <returns></returns>
        public async Task UpdateOnFly(string clientId, string userId, string accessToken, List<SubscriptionType> listOfSubs)
        {
            StartUp(clientId, userId, accessToken, listOfSubs);
            await _manager.UpdateOnFly(clientId, accessToken, _listOfSubs);
        }
        /// <summary>
        /// Provides Initialization part of function. Links all requested subscriptions to proper requests
        /// SetSubscriptionType may also support selective reward subscriptions. We are currently supporting only all reward sub.
        /// </summary>
        /// <param name="clientId"></param>
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
        public async Task Stop()
        {
            await _socket.DisconnectAsync();
            await _manager.Stop();
        }
        /// <summary>
        /// This event is triggered early in communication and provides session id, which is critical for proper function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        private async Task SocketOnRegisterSubscriptions(object sender, string? sessionId)
        {
            await _manager.Setup(_clientId, _accessToken, sessionId, _listOfSubs);
            _manager.Start();
        }
        /// <summary>
        /// Provides all notifications to user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task SocketOnNotification(object sender, Messages.NotificationMessage.WebSocketNotificationPayload e)
        {
            switch (e.Event)
            {
                case UpdateNotificationEvent updateEvent:
                    await OnUpdateNotificationEvent.TryInvoke(sender, updateEvent);
                    break;

                case FollowEvent followEvent:
                    await OnFollowEvent.TryInvoke(sender, followEvent);
                    break;

                case SubscribeEndEvent subscribeEndEvent:
                    await OnSubscribeEndEvent.TryInvoke(sender, subscribeEndEvent);
                    break;

                case SubscribeEvent subscribeEvent:
                    await OnSubscribeEvent.TryInvoke(sender, subscribeEvent);
                    break;

                case SubscriptionGiftEvent subscriptionGiftEvent:
                    await OnSubscriptionGiftEvent.TryInvoke(sender, subscriptionGiftEvent);
                    break;

                case SubscriptionMessageEvent subscriptionMessageEvent:
                    await OnSubscriptionMessageEvent.TryInvoke(sender, subscriptionMessageEvent);
                    break;

                case CheerEvent cheerEvent:
                    await OnCheerEvent.TryInvoke(sender, cheerEvent);
                    break;

                case RaidEvent raidEvent:
                    await OnRaidEvent.TryInvoke(sender, raidEvent);
                    break;

                case BanEvent banEvent:
                    await OnBanEvent.TryInvoke(sender, banEvent);
                    break;

                case UnBanEvent unBanEvent:
                    await OnUnBanEvent.TryInvoke(sender, unBanEvent);
                    break;

                case ModeratorRemoveEvent moderatorRemoveEvent:
                    await OnModeratorRemoveEvent.TryInvoke(sender, moderatorRemoveEvent);
                    break;

                case ModeratorAddEvent moderatorAddEvent:
                    await OnModeratorAddEvent.TryInvoke(sender, moderatorAddEvent);
                    break;

                case GuestStarSessionEndEvent guestStarSessionEndEvent:
                    await OnGuestStarSessionEndEvent.TryInvoke(sender, guestStarSessionEndEvent);
                    break;

                case GuestStarSessionBeginEvent guestStarSessionBeginEvent:
                    await OnGuestStarSessionBeginEvent.TryInvoke(sender, guestStarSessionBeginEvent);
                    break;


                case GuestStarGuestUpdateEvent guestStarGuestUpdateEvent:
                    await OnGuestStarGuestUpdateEvent.TryInvoke(sender, guestStarGuestUpdateEvent);
                    break;

                case GuestStarSlotUpdateEvent guestStarSlotUpdateEvent:
                    await OnGuestStarSlotUpdateEvent.TryInvoke(sender, guestStarSlotUpdateEvent);
                    break;

                case GuestStarSettingsUpdateEvent guestStarSettingsUpdateEvent:
                    await OnGuestStarSettingsUpdateEvent.TryInvoke(sender, guestStarSettingsUpdateEvent);
                    break;

                case PointsCustomRewardUpdateEvent customRewardUpdateEvent:
                    await OnPointsCustomRewardUpdateEvent.TryInvoke(sender, customRewardUpdateEvent);
                    break;

                case PointsCustomRewardRemoveEvent customRewardRemoveEvent:
                    await OnPointsCustomRewardRemoveEvent.TryInvoke(sender, customRewardRemoveEvent);
                    break;

                case PointsCustomRewardRedemptionUpdateEvent customRewardRedemptionUpdateEvent:
                    await OnPointsCustomRewardRedemptionUpdateEvent.TryInvoke(sender, customRewardRedemptionUpdateEvent);
                    break;

                case PointsCustomRewardRedemptionAddEvent customRewardRedemptionAddEvent:
                    await OnPointsCustomRewardRedemptionAddEvent.TryInvoke(sender, customRewardRedemptionAddEvent);
                    break;

                case PointsCustomRewardAddEvent customRewardAddEvent:
                    await OnPointsCustomRewardAddEvent.TryInvoke(sender, customRewardAddEvent);
                    break;

                case PollProgressEvent pollProgressEvent:
                    await OnPollProgressEvent.TryInvoke(sender, pollProgressEvent);
                    break;

                case PollEndEvent pollEndEvent:
                    await OnPollEndEvent.TryInvoke(sender, pollEndEvent);
                    break;

                case PollBeginEvent pollBeginEvent:
                    await OnPollBeginEvent.TryInvoke(sender, pollBeginEvent);
                    break;

                case PredictionProgressEvent predictionProgressEvent:
                    await OnPredictionProgressEvent.TryInvoke(sender, predictionProgressEvent);
                    break;

                case PredictionLockEvent predictionLockEvent:
                    await OnPredictionLockEvent.TryInvoke(sender, predictionLockEvent);
                    break;

                case PredictionEndEvent predictionEndEvent:
                    await OnPredictionEndEvent.TryInvoke(sender, predictionEndEvent);
                    break;

                case PredictionBeginEvent predictionBeginEvent:
                    await OnPredictionBeginEvent.TryInvoke(sender, predictionBeginEvent);
                    break;

                case HypeTrainProgressEvent hypeTrainProgressEvent:
                    await OnHypeTrainProgressEvent.TryInvoke(sender, hypeTrainProgressEvent);
                    break;

                case HypeTrainEndEvent hypeTrainEndEvent:
                    await OnHypeTrainEndEvent.TryInvoke(sender, hypeTrainEndEvent);
                    break;

                case HypeTrainBeginEvent hypeTrainBeginEvent:
                    await OnHypeTrainBeginEvent.TryInvoke(sender, hypeTrainBeginEvent);
                    break;

                case CharityCampaignStartEvent charityCampaignStartEvent:
                    await OnCharityCampaignStartEvent.TryInvoke(sender, charityCampaignStartEvent);
                    break;

                case CharityCampaignProgressEvent charityCampaignProgressEvent:
                    await OnCharityCampaignProgressEvent.TryInvoke(sender, charityCampaignProgressEvent);
                    break;

                case CharityCampaignStopEvent charityCampaignStopEvent:
                    await OnCharityCampaignStopEvent.TryInvoke(sender, charityCampaignStopEvent);
                    break;

                case CharityDonationEvent charityDonationEvent:
                    await OnCharityDonationEvent.TryInvoke(sender, charityDonationEvent);
                    break;

                //Cant be used by websocket
                //case DropEntitlementGrantEvent dropEntitlementGrantEvent:
                //    await OnDropEntitlementGrantEvent.TryInvoke(sender, dropEntitlementGrantEvent);
                //    break;

                //case ExtensionBitsTransactionCreateEvent bitsTransactionCreateEvent:
                //    await OnExtensionBitsTransactionCreateEvent.TryInvoke(sender, bitsTransactionCreateEvent);
                //    break;


                case GoalProgressEvent goalProgressEvent:
                    await OnGoalProgressEvent.TryInvoke(sender, goalProgressEvent);
                    break;

                case GoalEndEvent goalEndEvent:
                    await OnGoalEndEvent.TryInvoke(sender, goalEndEvent);
                    break;

                case GoalBeginEvent goalBeginEvent:
                    await OnGoalBeginEvent.TryInvoke(sender, goalBeginEvent);
                    break;

                case ShieldModeEndEvent shieldModeEndEvent:
                    await OnShieldModeEndEvent.TryInvoke(sender, shieldModeEndEvent);
                    break;

                case ShieldModeBeginEvent shieldModeBeginEvent:
                    await OnShieldModeBeginEvent.TryInvoke(sender, shieldModeBeginEvent);
                    break;

                case ShoutoutCreateEvent shoutoutCreateEvent:
                    await OnShoutoutCreateEvent.TryInvoke(sender, shoutoutCreateEvent);
                    break;

                case ShoutoutReceivedEvent shoutoutReceivedEvent:
                    await OnShoutoutReceivedEvent.TryInvoke(sender, shoutoutReceivedEvent);
                    break;

                case StreamOnlineEvent streamOnlineEvent:
                    await OnStreamOnlineEvent.TryInvoke(sender, streamOnlineEvent);
                    break;

                case StreamOfflineEvent streamOfflineEvent:
                    await OnStreamOfflineEvent.TryInvoke(sender, streamOfflineEvent);
                    break;
                //Cant be used by websocket

                //case UserAuthorizationGrantEvent userAuthorizationGrantEvent:
                //    await OnUserAuthorizationGrantEvent.TryInvoke(sender, userAuthorizationGrantEvent);
                //    break;

                //case UserAuthorizationRevokeEvent userAuthorizationRevokeEvent:
                //    await OnUserAuthorizationRevokeEvent.TryInvoke(sender, userAuthorizationRevokeEvent);
                //    break;

                //User update doesnt maintain proper structure. 
                //NOT SUPPORTED

                //case UserUpdateEvent userUpdateEvent:
                //    await OnUserUpdateEvent.TryInvoke(sender, userUpdateEvent);
                //    break;



                default:
                    throw new NotImplementedException();
            }
        }

    }
}
