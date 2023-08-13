using System.Net.WebSockets;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.CompilerServices;
using Twitch.EventSub.API;
using Twitch.EventSub.API.Models;
using Twitch.EventSub.CoreFunctions;
using Twitch.EventSub.Library.CoreFunctions;

namespace Twitch.EventSub
{
    public class EventSubscriptionManager
    {
        private readonly TwitchParcialApi _api;
        private string? _sessionId;
        private readonly ILogger _logger;
        private string? _clientId;
        private string? _accessToken;
        private bool _checkRunning;
        private bool _setup;
        private PeriodicTimer? _timer;
        private Task _backgroundTask;
        private bool _isRunning;
        private CancellationTokenSource _cancelSource;

        private List<CreateSubscriptionRequest>? _requestedSubscriptions;

        public event AsyncEventHandler<InvalidAccessTokenException> OnRefreshTokenRequest;

        public EventSubscriptionManager(ILogger logger, ILogger ApiLogger)
        {
            _api = new TwitchParcialApi(ApiLogger);
            _logger = logger;
        }

        /// <summary>
        /// This function serves for Connect and Reconnect cases
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="accessToken"></param>
        /// <param name="sessionId"></param>
        /// <param name="requestedSubscriptions"></param>
        /// <returns></returns>
        public async Task Setup(string? clientId, string? accessToken, string? sessionId, List<CreateSubscriptionRequest>? requestedSubscriptions)
        {
            
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            if (requestedSubscriptions == null)
            {
                throw new NullReferenceException();
            }
            foreach (var typeListOfSub in requestedSubscriptions)
            {
                typeListOfSub.Transport.SessionId = _sessionId;
            }
            while (_checkRunning)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(300));
            }
            _requestedSubscriptions = requestedSubscriptions;
            _setup = true;
            _logger.LogInformation("Event Sub ready: Client Id: " + clientId + " accessToken " + accessToken + " session id" + sessionId);
        }
        /// <summary>
        /// This Function servers for handeling AccessToken changes ONLY
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="accessToken"></param>
        /// <param name="requestedSubscriptions"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Setup was not run</exception>
        /// <exception cref="Exception">May crash, if dupes found in list</exception>
        public async Task UpdateOnFly(string? clientId, string? accessToken, List<CreateSubscriptionRequest>? requestedSubscriptions)
        {
            if (!_setup)
            {
                throw new InvalidOperationException();
            }
            if (_sessionId == null)
            {
                throw new NullReferenceException();

            }

            _clientId = clientId;
            _accessToken = accessToken;
            if (requestedSubscriptions != null)
            {
                foreach (var typeListOfSub in requestedSubscriptions)
                {
                    typeListOfSub.Transport.SessionId = _sessionId;
                }
                while (_checkRunning)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(300));
                }
                _requestedSubscriptions = requestedSubscriptions;
            }
            await RunCheck(_cancelSource);
        }
        /// <summary>
        /// Creates repeated cycle for checking subscriptons. Even tho we should know about every change in
        /// subs revoke it serves as sanity check.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IncompleteInitialization">Setup was not run</exception>
        /// <exception cref="Exception">May crash, if dupes found in list</exception>
        /// <exception cref="ArgumentNullException">May crash for check sub has no valid values to run on</exception>
        public void Start()
        {
            if (!_setup)
            {
                throw new IncompleteInitialization();
            }

            if (_isRunning)
            {
                return;
            }
            _cancelSource = new CancellationTokenSource();
            _isRunning = true;
            //Subscriptions decay every hour, having 30 min is just to be on safe side

            _backgroundTask = Task.Run(BackgroundCheck,_cancelSource.Token);
        }

        public async Task RevocationResolver(Messages.RevocationMessage.WebSocketRevocationMessage e)
        {
            if (_requestedSubscriptions == null || _clientId == null || _accessToken == null)
            {
                return;
            }
            foreach (var sub in _requestedSubscriptions.Where(
                         sub => sub.Type == e?.Payload?.Type && sub.Version == e?.Payload?.Version))
            {
                if (!await ApiTrySubscribeAsync(_clientId, _accessToken, sub, _cancelSource))
                {
                    _logger.LogInformation("Failed to subscribe subscription during revocation");
                }
                _logger.LogInformation("Refreshed sub due revocation: " + sub.Type + "caused by " + e?.Payload?.Status);
            }

        }

        private async Task BackgroundCheck()
        {
            _timer = new PeriodicTimer(TimeSpan.FromMinutes(30));
            try
            {
                do
                {
                    await RunCheck(_cancelSource);
                    _logger.LogInformation("Check run.");
                } while (await _timer.WaitForNextTickAsync(_cancelSource.Token));
            }
            catch (TaskCanceledException)
            {
                // proceed
            }
        }

        /// <summary>
        /// Stops repeated checking
        /// </summary>
        /// <returns></returns>
        public async Task Stop()
        {
            if (!_setup)
            {
                _timer?.Dispose();
                return;
            }
            if (_isRunning)
            {
                _isRunning = false;
                await Clear(_cancelSource);
                _cancelSource.Cancel();
                _timer?.Dispose();
            }
        }
        /// <summary>
        /// Removes all subs, it has to be unsubscribed for reasons of cost dependent on session id
        /// </summary>
        /// <returns></returns>
        private async Task Clear(CancellationTokenSource clSource)
        {
            if (_clientId == null || _accessToken == null)
            {
                throw new ArgumentNullException();
            }
            var allSubscriptions = await ApiTryGetAllSubscriptionsAsync(_clientId, _accessToken, clSource, StatusProvider.SubscriptionStatus.Empty);
            foreach (var getSubscriptionsResponse in allSubscriptions)
            {
                foreach (var subscription in getSubscriptionsResponse.Data)
                {
                    if (!await ApiTryUnSubscribeAsync(_clientId, _accessToken, subscription.Id, clSource))
                    {
                        _logger.LogInformation("Failed to unsubscribe during clear" + subscription.Type);
                    }
                    _logger.LogInformation("Sub cleared: " + subscription.Type);
                }
            }
        }
        /// <summary>
        /// First compares dictionary of requested subs to test on dupes, then removes all old or inactive subs,
        /// Then compares current enabled subs to requested dictionary. Removes unwanted subs and adds missing.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IncompleteInitialization">Setup was not run</exception>
        /// <exception cref="Exception">Trigger when requested list has dupes</exception>
        public async Task RunCheck(CancellationTokenSource clSource)
        {
            if (_checkRunning)
            {
                // await until check is done
                return;
            }
            if (!_setup)
            {
                throw new IncompleteInitialization();
            }
            if (_requestedSubscriptions == null)
            {
                return;
            }
            _checkRunning = true;
            //test for dupes
            if (_requestedSubscriptions
                .GroupBy(reqSub => new
                {
                    reqSub.Type
                }).Any(group => group.Count() > 1))
            {
                throw new Exception("List contains dupes");
            }

            //remove old connections, old sessions and all subscriptions with error status
            if (_clientId == null || _accessToken == null)
            {
                throw new ArgumentNullException();
            }
            var allSubscriptions = await ApiTryGetAllSubscriptionsAsync(_clientId, _accessToken, clSource, StatusProvider.SubscriptionStatus.Empty);
            //Yes we can get null from subscription function, if something goes horribly wrong.
            if (allSubscriptions == null)
            {
                _logger.LogInformation("Subscription function returned null, skipping check");
                return;
            }
            foreach (var getSubscriptionsResponse in allSubscriptions)
            {
                foreach (var subscription in getSubscriptionsResponse.Data)
                {
                    if (subscription.Transport.SessionId != _sessionId || subscription.Status != "enabled" ||
                        DateTime.UtcNow - ReplayProtection.ConvertToRfc3339WithNanoseconds(subscription.CreatedAt) > TimeSpan.FromHours(1))
                    {
                        if (!await ApiTryUnSubscribeAsync(_clientId, _accessToken, subscription.Id, clSource))
                        {
                            _logger.LogInformation("Failed to unsubscribe during check" + subscription.Type);
                        }
                        _logger.LogInformation("Cleared subscription:" + subscription.Type);
                    }
                }
            }

            //Rerun subscription search to get all active current session subs
            allSubscriptions = await ApiTryGetAllSubscriptionsAsync(_clientId, _accessToken, clSource, StatusProvider.SubscriptionStatus.Empty);
            //Yes we can get null from subscription function, if something goes horribly wrong.
            if (allSubscriptions == null)
            {
                _logger.LogInformation("Subscription function returned null, skipping check");
                return;
            }
            foreach (var getSubscriptionsResponse in allSubscriptions)
            {
                var activeSubscriptions = getSubscriptionsResponse.Data;

                // Find subscriptions that are extra (present in activeSubscriptions but not in _requestedSubscriptions)
                var extraSubscriptions = activeSubscriptions
                .Where(subscription => !_requestedSubscriptions.Any(reqSub =>
                reqSub.Type == subscription.Type && reqSub.Version == subscription.Version)).ToList();

                // Find subscriptions that are missing (present in _requestedSubscriptions but not in activeSubscriptions)
                var missingSubscriptions = _requestedSubscriptions
                .Where(reqSub => !activeSubscriptions.Any(subscription =>
                subscription.Type == reqSub.Type && subscription.Version == reqSub.Version)).ToList();

                // Handle extra and missing subscriptions
                if (extraSubscriptions.Any())
                {
                    // Perform your logic here for extra subscriptions
                    foreach (var extraSubscription in extraSubscriptions)
                    {
                        if (await ApiTryUnSubscribeAsync(_clientId, _accessToken, extraSubscription.Id, clSource))
                        {
                            _logger.LogInformation("Failed to unsubscribe active subscription during check" + extraSubscription.Type);
                        }
                        _logger.LogInformation("Removed extra sub: " + extraSubscription.Type);
                    }
                }

                if (missingSubscriptions.Any())
                {
                    // Perform your logic here for missing subscriptions
                    foreach (var missingSubscription in missingSubscriptions)
                    {
                        if (!await ApiTrySubscribeAsync(_clientId, _accessToken, missingSubscription, clSource))
                        {
                            _logger.LogInformation("Failed to subscribe subscription during check");
                        }
                        _logger.LogInformation("Added extra sub: " + missingSubscription.Type);
                    }
                }
            }
            _checkRunning = false;
        }

        public async Task<bool> ApiTrySubscribeAsync(string clientId, string accessToken, CreateSubscriptionRequest create, CancellationTokenSource clSource)
        {
            async Task<bool> TrySubscribe() => await _api.SubscribeAsync(clientId, accessToken, create, clSource);
            return await TryFuncAsync(TrySubscribe);
        }
        public async Task<bool> ApiTryUnSubscribeAsync(string clientId, string accessToken, string subId, CancellationTokenSource clSource)
        {
            async Task<bool> TryUnSubscribe() => await _api.UnSubscribeAsync(clientId, accessToken, subId, clSource);
            return await TryFuncAsync(TryUnSubscribe);
        }
        public async Task<List<GetSubscriptionsResponse>> ApiTryGetAllSubscriptionsAsync(string clientId, string accessToken,CancellationTokenSource clSource, StatusProvider.SubscriptionStatus statusSelector)
        {
            async Task<List<GetSubscriptionsResponse>> TryGetAllSubscriptionsAsync() => await _api.GetAllSubscriptionsAsync(clientId, accessToken,clSource ,statusSelector);
            return await TryFuncAsync(TryGetAllSubscriptionsAsync);
        }


        /// <summary>
        /// This should catch any AccessToken exception and calls outside for changes.
        /// Then it calls function again.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="apiCallAction"></param>
        /// <returns></returns>
        private async Task<TType> TryFuncAsync<TType>(Func<Task<TType>> apiCallAction)
        {
            try
            {
                return await apiCallAction();
            }
            catch (InvalidAccessTokenException ex)
            {
                //procedure must run UpdateOnFly function for proper change
                await OnRefreshTokenRequest.TryInvoke(this, ex);
                return await apiCallAction();

            }
            catch (TaskCanceledException)
            {
                //its alright, move on
            }
            catch (Exception ex)
            {
                _logger.LogError("Api call failed due to: {ex}", ex);
            }
            //This is expected behavior. If we get null or false, we handle it in higher part of fuction
            return default;
        }
    }
}
