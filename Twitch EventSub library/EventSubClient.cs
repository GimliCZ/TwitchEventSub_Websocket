using Microsoft.Extensions.Logging;
using Twitch_EventSub_library.API;
using Twitch_EventSub_library.API.Extensions;
using Twitch_EventSub_library.API.Models;
using Twitch_EventSub_library.CoreFunctions;

namespace Twitch_EventSub_library
{
    public class EventSubClient
    {
        private readonly ILogger<EventSubClient> _logger;
        private readonly EventSubscriptionManager _manager;
        private readonly EventSubSocketWrapper _socket;
        private string _clientId;
        private string _accessToken;
        private List<CreateSubscriptionRequest> _listOfSubs;
        private bool _revocationRunning = false;

        public event EventHandler<string> OnUnexpectedConnectionTermination;
        public event AsyncEventHandler<InvalidAccessTokenException> OnRefreshToken;
        public EventSubClient(ILogger<TwitchParcialApi> apiLogger, ILogger<EventSubSocketWrapper> socketWrLogger,
            ILogger<GenericWebsocket> socketLogger, ILogger<Watchdog> watchdogLogger, ILogger<EventSubClient> Logger,
            ILogger<EventSubscriptionManager> managerLogger)

        {
            _logger = Logger;
            _manager = new EventSubscriptionManager(managerLogger, apiLogger);
            _socket = new EventSubSocketWrapper(socketWrLogger, socketLogger, watchdogLogger, TimeSpan.FromMilliseconds(300));
            _socket.OnNotificationMessage += SocketOnNotification;
            _socket.OnRegisterSubscriptions += SocketOnRegisterSubscriptions;
            _socket.OnRevocationMessage += SocketOnRevocationMessage;
            _socket.OnOutsideDisconnect += SocketOnOutsideDisconnect;
            _manager.OnRefreshTokenRequest += ManagerOnRefreshTokenRequest;
        }

        private async Task ManagerOnRefreshTokenRequest(object sender, InvalidAccessTokenException e)
        {
            //I know, this is suboptimal you can subscribe ManagerOnRefreshTokenRequest without further skip
            await OnRefreshToken.Invoke(this, e);
        }

        /// <summary>
        ///  Wrapper got into unexpected connection termination. Triggers Manager to clean up and stop repeating checks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SocketOnOutsideDisconnect(object sender, string e)
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
            if (_revocationRunning)
            {
                return;
            }
            _revocationRunning = true;
            await Task.Delay(TimeSpan.FromSeconds(2));
            await _manager.RunCheck();
            _revocationRunning = false;
        }
        /// <summary>
        /// Initializes connection to event sub servers.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="accessToken"></param>
        /// <param name="listOfSubs"></param>
        /// <returns></returns>
        public async Task<bool> Start(string clientId, string accessToken, List<SubscriptionType> listOfSubs)
        {
            StartUp(clientId, accessToken, listOfSubs);
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
        public async Task UpdateOnFly(string clientId, string accessToken, List<SubscriptionType> listOfSubs)
        {
            StartUp(clientId, accessToken, listOfSubs);
            await _manager.UpdateOnFly(clientId, accessToken, _listOfSubs);
        }
        /// <summary>
        /// Provides Initialization part of function. Links all requested subscriptions to proper requests
        /// SetSubscriptionType may also support selective reward subscriptions. We are currently supporting only all reward sub.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="accessToken"></param>
        /// <param name="listOfSubs"></param>
        private void StartUp(string clientId, string accessToken, List<SubscriptionType> listOfSubs)
        {
            _clientId = clientId;
            _accessToken = accessToken;
            _listOfSubs = new List<CreateSubscriptionRequest>();
            foreach (var type in listOfSubs)
            {
                _listOfSubs.Add(new CreateSubscriptionRequest()
                {
                    Condition = new Condition() { UserId = clientId },
                    Transport = new Transport() { Method = "websocket" }
                }.SetSubscriptionType(type, clientId));

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
        private async Task SocketOnRegisterSubscriptions(object sender, string sessionId)
        {
            await _manager.Setup(_clientId, _accessToken, sessionId, _listOfSubs);
            await _manager.Start();
        }
        //TODO: Implement
        /// <summary>
        /// Provides all notifications to user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Task SocketOnNotification(object sender, Messages.NotificationMessage.WebSocketNotificationPayload e)
        {
            // throw new NotImplementedException();
            return Task.CompletedTask;
        }

    }
}
