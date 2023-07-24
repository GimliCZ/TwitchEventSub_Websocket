using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.CompilerServices;
using Twitch_EventSub_library.API;
using Twitch_EventSub_library.API.Models;
using Twitch_EventSub_library.CoreFunctions;

namespace Twitch_EventSub_library
{
    public class EventSubscriptionManager
    {
        private readonly TwitchParcialApi _api;
        private string? _sessionId;
        private readonly ILogger<EventSubscriptionManager> _logger;
        private string? _clientId;
        private string? _accessToken;
        private bool _checkRunning;
        private bool _setup;
        private PeriodicTimer? _timer;
        private bool _isRunning;

        private List<CreateSubscriptionRequest>? _requestedSubscriptions;

        public event AsyncEventHandler<InvalidAccessTokenException> OnRefreshTokenRequest;

        public EventSubscriptionManager(ILogger<EventSubscriptionManager> logger, ILogger<TwitchParcialApi> ApiLogger)
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

            _clientId = clientId;
            _accessToken = accessToken;
            foreach (var typeListOfSub in requestedSubscriptions)
            {
                typeListOfSub.Transport.SessionId = _sessionId;
            }
            while (_checkRunning)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(300));
            }
            _requestedSubscriptions = requestedSubscriptions;
            await RunCheck();
        }
        /// <summary>
        /// Creates repeated cycle for checking subscriptons. Even tho we should know about every change in
        /// subs revoke it serves as sanity check.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IncompleteInitialization">Setup was not run</exception>
        /// <exception cref="Exception">May crash, if dupes found in list</exception>
        public async Task Start()
        {
            if (!_setup)
            {
                throw new IncompleteInitialization();
            }
            _isRunning = true;
            if (_isRunning)
            {
                return;
            }
            //Subscriptions decay every hour, having 30 min is just to be on safe side
            _timer = new PeriodicTimer(TimeSpan.FromMinutes(30));
            try
            {
                while (await _timer.WaitForNextTickAsync())
                {
                    await RunCheck();
                }
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
            _isRunning = false;
            await Clear();
            _timer?.Dispose();
        }
        /// <summary>
        /// Removes all subs
        /// </summary>
        /// <returns></returns>
        private async Task Clear()
        {
            var allSubscriptions = await ApiTryGetAllSubscriptionsAsync(_clientId, _accessToken, StatusProvider.SubscriptionStatus.Empty);
            foreach (var getSubscriptionsResponse in allSubscriptions)
            {
                foreach (var subscription in getSubscriptionsResponse.Data)
                {
                    if (await ApiTryUnSubscribeAsync(_clientId, _accessToken, subscription.Id))
                    {
                        _logger.LogInformation("Failed to unsubscribe during clear" + subscription.Type);
                    }
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
        public async Task RunCheck()
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
            var allSubscriptions = await ApiTryGetAllSubscriptionsAsync(_clientId, _accessToken, StatusProvider.SubscriptionStatus.Empty);
            foreach (var getSubscriptionsResponse in allSubscriptions)
            {
                foreach (var subscription in getSubscriptionsResponse.Data)
                {
                    if (subscription.Transport.SessionId != _sessionId ||
                        DateTimeOffset.Now - ReplayProtection.ConvertToRfc3339WithNanoseconds(subscription.CreatedAt) > TimeSpan.FromHours(1) ||
                        subscription.Status != "enabled")
                    {
                        if (await ApiTryUnSubscribeAsync(_clientId, _accessToken, subscription.Id))
                        {
                            _logger.LogInformation("Failed to unsubscribe during check" + subscription.Type);
                        }

                    }
                }
            }

            //Rerun subscription search to get all active current session subs
            allSubscriptions = await _api.GetAllSubscriptionsAsync(_clientId, _accessToken, StatusProvider.SubscriptionStatus.Empty);
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
                        if (await ApiTryUnSubscribeAsync(_clientId, _accessToken, extraSubscription.Id))
                        {
                            _logger.LogInformation("Failed to unsubscribe active subscription during check" + extraSubscription.Type);
                        }
                    }
                }

                if (missingSubscriptions.Any())
                {
                    // Perform your logic here for missing subscriptions
                    foreach (var missingSubscription in missingSubscriptions)
                    {
                        if (await ApiTrySubscribeAsync(_clientId, _accessToken, missingSubscription))
                        {
                            _logger.LogInformation("Failed to subscribe subscription during check");
                        }
                    }
                }
            }
            _checkRunning = false;
        }

        public async Task<bool> ApiTrySubscribeAsync(string clientId, string accessToken, CreateSubscriptionRequest create)
        {
            async Task<bool> TrySubscribe() => await _api.SubscribeAsync(clientId, accessToken, create);
            return await TryFuncAsync(TrySubscribe);
        }
        public async Task<bool> ApiTryUnSubscribeAsync(string clientId, string accessToken, string subId)
        {
            async Task<bool> TryUnSubscribe() => await _api.UnSubscribeAsync(clientId, accessToken, subId);
            return await TryFuncAsync(TryUnSubscribe);
        }
        public async Task<List<GetSubscriptionsResponse>> ApiTryGetAllSubscriptionsAsync(string clientId, string accessToken, StatusProvider.SubscriptionStatus statusSelector)
        {
            async Task<List<GetSubscriptionsResponse>> TryGetAllSubscriptionsAsync() => await _api.GetAllSubscriptionsAsync(clientId, accessToken, statusSelector);
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
                return await TryFuncAsync(apiCallAction);
            }
            catch (InvalidAccessTokenException ex)
            {
                //procedure must run UpdateOnFly function for proper change
                await OnRefreshTokenRequest.Invoke(this, ex);
                return await apiCallAction();

            }
            catch (Exception ex)
            {
                _logger.LogError("Api call failed due to: {ex}", ex);
            }

            return default;
        }
    }
}
