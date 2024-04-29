using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.CompilerServices;
using Twitch.EventSub.API;
using Twitch.EventSub.API.Models;
using Twitch.EventSub.CoreFunctions;
using Twitch.EventSub.Interfaces;
using Twitch.EventSub.Library.CoreFunctions;

namespace Twitch.EventSub
{
    public class EventSubscriptionManager : IEventSubscriptionManager
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

        public event AsyncEventHandler<InvalidAccessTokenException> OnRefreshTokenRequestAsync;

        public EventSubscriptionManager(ILogger logger)
        {
            _api = new TwitchParcialApi(logger);
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
        public async Task SetupAsync(string? clientId, string? accessToken, string? sessionId, List<CreateSubscriptionRequest>? requestedSubscriptions)
        {

            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
            _sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            if (requestedSubscriptions == null)
            {
                throw new ArgumentNullException(nameof(requestedSubscriptions));
            }
            foreach (var typeListOfSub in requestedSubscriptions)
            {
                typeListOfSub.Transport.SessionId = _sessionId;
            }
            await WaitUntilCheckCompletion();
            _requestedSubscriptions = requestedSubscriptions;
            _setup = true;
            _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager]  Event Sub ready: Client Id: " + clientId + " accessToken " + accessToken + " session id" + sessionId);
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
        public async Task UpdateOnFlyAsync(string? clientId, string? accessToken, List<CreateSubscriptionRequest>? requestedSubscriptions)
        {
            if (!_setup)
            {
                throw new InvalidOperationException();
            }
            if (_sessionId == null)
            {
                throw new ArgumentNullException(nameof(_sessionId));

            }

            _clientId = clientId;
            _accessToken = accessToken;
            if (requestedSubscriptions != null)
            {
                foreach (var typeListOfSub in requestedSubscriptions)
                {
                    typeListOfSub.Transport.SessionId = _sessionId;
                }
                await WaitUntilCheckCompletion();
                _requestedSubscriptions = requestedSubscriptions;
            }
            await RunCheckAsync(_cancelSource);
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
                _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Subscription Manager was attempted to run multiple times.");
                return;
            }
            _cancelSource = new CancellationTokenSource();
            _isRunning = true;
            //Subscriptions decay every hour, having 30 min is just to be on safe side

            _backgroundTask = Task.Run(BackgroundCheckAsync, _cancelSource.Token);
        }

        public async Task RevocationResolverAsync(Messages.RevocationMessage.WebSocketRevocationMessage e)
        {
            if (_requestedSubscriptions == null || _clientId == null || _accessToken == null)
            {
                _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Revocation Resolver got subscriptions, clientId or accessToken as Null");
                return;
            }
            foreach (var sub in _requestedSubscriptions.Where(
                         sub => sub.Type == e?.Payload?.Type && sub.Version == e?.Payload?.Version))
            {
                if (!await ApiTrySubscribeAsync(_clientId, _accessToken, sub, _cancelSource))
                {
                    _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Failed to subscribe subscription during revocation");
                }
                _logger.LogInformationDetails("[EventSubClient] - [EventSubscriptionManager] Refreshed sub due revocation: " + sub.Type + "caused by ", e);
            }

        }

        private async Task BackgroundCheckAsync()
        {
            _timer = new PeriodicTimer(TimeSpan.FromMinutes(30));
            try
            {
                do
                {
                    await RunCheckAsync(_cancelSource);
                    _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Check run.");
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
        public async Task StopAsync()
        {
            if (!_setup)
            {
                throw new InvalidOperationException();
            }
            if (_isRunning)
            {
                _isRunning = false;
                await ClearAsync(_cancelSource);
                _cancelSource.Cancel();
                _timer?.Dispose();
            }
            else
            {
                _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Subscription Manager was already stopped.");
            }
        }
        /// <summary>
        /// Removes all subs, it has to be unsubscribed for reasons of cost dependent on session id
        /// </summary>
        /// <returns></returns>
        private async Task ClearAsync(CancellationTokenSource clSource)
        {
            if (_clientId == null)
            {
                throw new ArgumentNullException(nameof(_clientId));
            }
            if (_accessToken == null)
            {
                throw new ArgumentNullException(nameof(_accessToken));
            }
            var allSubscriptions = await ApiTryGetAllSubscriptionsAsync(_clientId, _accessToken, clSource, StatusProvider.SubscriptionStatus.Empty);
            if (allSubscriptions is null) {
                return;
            }
            foreach (var getSubscriptionsResponse in allSubscriptions)
            {
                if (getSubscriptionsResponse is null || getSubscriptionsResponse.Data is null)
                {
                    _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Retrieved null Subscription Response");
                    continue;
                }

                foreach (var subscription in getSubscriptionsResponse.Data)
                {

                    if (subscription is null)
                    {
                        _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Retrieved null Subscription");
                        continue;
                    }

                    if (!await ApiTryUnSubscribeAsync(_clientId, _accessToken, subscription.Id, clSource))
                    {
                        _logger.LogWarningDetails("[EventSubClient] - [EventSubscriptionManager] Failed to unsubscribe during clear", subscription);
                        continue;
                    }
                    _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Sub cleared: " + subscription.Type);
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
        private async Task RunCheckAsync(CancellationTokenSource clSource)
        {
            if (_checkRunning)
            {
                // await until check is done
                _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Check tried to run while already running.");
                return;
            }
            if (!_setup)
            {
                throw new IncompleteInitialization();
            }
            if (_requestedSubscriptions == null)
            {
                _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Requested subscriptions returned Null");
                return;
            }
            try
            {
                _checkRunning = true;
                await RunCheckInternalAsync(clSource);
            }
            catch(Exception)
            {
                throw;
            }
            finally
            {
                _checkRunning = false;
            }
        }

        private async Task RunCheckInternalAsync(CancellationTokenSource clSource)
        {
            //test for dupes
            // DOC: _requestedSubscriptions is always non null type
            if (_requestedSubscriptions!
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
                _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Subscription function returned null, skipping check");
                return;
            }
            foreach (var getSubscriptionsResponse in allSubscriptions)
            {
                foreach (var subscription in getSubscriptionsResponse.Data)
                {
                    if (subscription.Transport.SessionId != _sessionId || subscription.Status != "enabled" ||
                        DateTime.UtcNow - ReplayProtection.ParseDateTimeString(subscription.CreatedAt) > TimeSpan.FromHours(1))
                    {
                        if (!await ApiTryUnSubscribeAsync(_clientId, _accessToken, subscription.Id, clSource))
                        {
                            _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Failed to unsubscribe during check" + subscription.Type);
                        }
                        _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Cleared subscription:" + subscription.Type);
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
                .Where(subscription => !_requestedSubscriptions!.Any(reqSub =>
                reqSub.Type == subscription.Type && reqSub.Version == subscription.Version)).ToList();

                // Find subscriptions that are missing (present in _requestedSubscriptions but not in activeSubscriptions)
                var missingSubscriptions = _requestedSubscriptions!
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
                            _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Failed to unsubscribe active subscription during check" + extraSubscription.Type);
                        }
                        _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Removed extra sub: " + extraSubscription.Type);
                    }
                }

                if (missingSubscriptions.Any())
                {
                    // Perform your logic here for missing subscriptions
                    foreach (var missingSubscription in missingSubscriptions)
                    {
                        if (!await ApiTrySubscribeAsync(_clientId, _accessToken, missingSubscription, clSource))
                        {
                            _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Failed to subscribe subscription during check");
                        }
                        _logger.LogInformation("[EventSubClient] - [EventSubscriptionManager] Added extra sub: " + missingSubscription.Type);
                    }
                }
            }
        }

        private async Task<bool> ApiTrySubscribeAsync(string clientId, string accessToken, CreateSubscriptionRequest create, CancellationTokenSource clSource)
        {
            async Task<bool> TrySubscribe() => await _api.SubscribeAsync(clientId, accessToken, create, clSource);
            return await TryFuncAsync(TrySubscribe);
        }

        private async Task<bool> ApiTryUnSubscribeAsync(string clientId, string accessToken, string subId, CancellationTokenSource clSource)
        {
            async Task<bool> TryUnSubscribe() => await _api.UnSubscribeAsync(clientId, accessToken, subId, clSource);
            return await TryFuncAsync(TryUnSubscribe);
        }

        private async Task<List<GetSubscriptionsResponse>?> ApiTryGetAllSubscriptionsAsync(string clientId, string accessToken, CancellationTokenSource clSource, StatusProvider.SubscriptionStatus statusSelector)
        {
            async Task<List<GetSubscriptionsResponse>> TryGetAllSubscriptionsAsync() => await _api.GetAllSubscriptionsAsync(clientId, accessToken, clSource, statusSelector);
            return await TryFuncAsync(TryGetAllSubscriptionsAsync);
        }


        /// <summary>
        /// This should catch any AccessToken exception and calls outside for changes.
        /// Then it calls function again.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="apiCallAction"></param>
        /// <returns></returns>
        private async Task<TType?> TryFuncAsync<TType>(Func<Task<TType>> apiCallAction)
        {
            try
            {
                return await apiCallAction();
            }
            catch (InvalidAccessTokenException ex)
            {
                //procedure must run UpdateOnFly function for proper change
                await OnRefreshTokenRequestAsync.TryInvoke(this, ex);
                return await apiCallAction();

            }
            catch (TaskCanceledException)
            {
                //its alright, move on
            }
            catch (Exception ex)
            {
                _logger.LogInformationDetails("[EventSubClient] - [EventSubscriptionManager] Api call failed due to:", ex);
            }
            //This is expected behavior. If we get null or false, we handle it in higher part of function
            _logger.LogWarningDetails("[EventSubClient] - [EventSubscriptionManager] Try Func Async returned Default value.", apiCallAction.Method.Name);
            return default;
        }
        private async Task WaitUntilCheckCompletion()
        {
            CancellationTokenSource cls = new CancellationTokenSource();
            cls.CancelAfter(TimeSpan.FromSeconds(5));
            while (_checkRunning && !cls.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(300));
            }
        }
    }
}
