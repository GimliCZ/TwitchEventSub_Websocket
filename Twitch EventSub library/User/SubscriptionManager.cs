using Microsoft.Extensions.Logging;
using Twitch.EventSub.API;
using Twitch.EventSub.API.Models;
using Twitch.EventSub.CoreFunctions;

namespace Twitch.EventSub.User
{
    public class SubscriptionManager
    {
        public event AsyncEventHandler<InvalidAccessTokenException> OnRefreshTokenRequestAsync;
        public async Task<bool> RunCheckAsync(string userId, List<CreateSubscriptionRequest> requestedSubscriptions, string clientId, string accessToken, string sessionId, CancellationTokenSource clSource, ILogger logger)
        {
            foreach (var typeListOfSub in requestedSubscriptions)
            {
                typeListOfSub.Transport.SessionId = sessionId;
            }

            //test for dupes
            // DOC: _requestedSubscriptions is always non null type
            if (requestedSubscriptions!
                .GroupBy(reqSub => new
                {
                    reqSub.Type
                }).Any(group => group.Count() > 1))
            {
                throw new Exception("[EventSubClient] - [SubscriptionManager] - List contains dupes");
            }

            //remove old connections, old sessions and all subscriptions with error status
            if (clientId == null || accessToken == null)
            {
                throw new ArgumentNullException();
            }
            var allSubscriptions = await ApiTryGetAllSubscriptionsAsync(clientId, accessToken, userId, clSource, logger, StatusProvider.SubscriptionStatus.Empty);
            //Yes we can get null from subscription function, if something goes horribly wrong.
            if (allSubscriptions == null)
            {
                logger.LogInformation("[EventSubClient] - [SubscriptionManager] Subscription function returned null, skipping check");
                return false;
            }
            foreach (var getSubscriptionsResponse in allSubscriptions)
            {
                foreach (var subscription in getSubscriptionsResponse.Data)
                {
                    if (subscription.Transport.SessionId != sessionId || subscription.Status != "enabled" ||
                        DateTime.UtcNow - ReplayProtection.ParseDateTimeString(subscription.CreatedAt) > TimeSpan.FromHours(1))
                    {
                        if (!await ApiTryUnSubscribeAsync(clientId, accessToken, subscription.Id, userId, logger, clSource))
                        {
                            logger.LogInformation("[EventSubClient] - [SubscriptionManager] Failed to unsubscribe during check" + subscription.Type);
                            return false;
                        }
                        logger.LogInformation("[EventSubClient] - [SubscriptionManager] Cleared subscription:" + subscription.Type);
                    }
                }
            }

            //Rerun subscription search to get all active current session subs
            allSubscriptions = await ApiTryGetAllSubscriptionsAsync(clientId, accessToken, userId, clSource, logger, StatusProvider.SubscriptionStatus.Empty);
            //Yes we can get null from subscription function, if something goes horribly wrong.
            if (allSubscriptions == null)
            {
                logger.LogInformation("[EventSubClient] - [SubscriptionManager] Subscription function returned null, skipping check");
                return false;
            }
            foreach (var getSubscriptionsResponse in allSubscriptions)
            {
                var activeSubscriptions = getSubscriptionsResponse.Data;

                // Find subscriptions that are extra (present in activeSubscriptions but not in _requestedSubscriptions)
                var extraSubscriptions = activeSubscriptions
                .Where(subscription => !requestedSubscriptions!.Any(reqSub =>
                reqSub.Type == subscription.Type && reqSub.Version == subscription.Version)).ToList();

                // Find subscriptions that are missing (present in _requestedSubscriptions but not in activeSubscriptions)
                var missingSubscriptions = requestedSubscriptions!
                .Where(reqSub => !activeSubscriptions.Any(subscription =>
                subscription.Type == reqSub.Type && subscription.Version == reqSub.Version)).ToList();

                // Handle extra and missing subscriptions
                if (extraSubscriptions.Any())
                {
                    // Perform your logic here for extra subscriptions
                    foreach (var extraSubscription in extraSubscriptions)
                    {
                        if (await ApiTryUnSubscribeAsync(clientId, accessToken, extraSubscription.Id, userId, logger, clSource))
                        {
                            logger.LogInformation("[EventSubClient] - [SubscriptionManager] Failed to unsubscribe active subscription during check" + extraSubscription.Type);
                            return false;
                        }
                        logger.LogInformation("[EventSubClient] - [SubscriptionManager] Removed extra sub: " + extraSubscription.Type);
                    }
                }

                if (missingSubscriptions.Any())
                {
                    // Perform your logic here for missing subscriptions
                    foreach (var missingSubscription in missingSubscriptions)
                    {
                        if (!await ApiTrySubscribeAsync(clientId, accessToken, missingSubscription, userId, logger, clSource))
                        {
                            logger.LogInformation("[EventSubClient] - [SubscriptionManager] Failed to subscribe subscription during check");
                            return false;
                        }
                        logger.LogInformation("[EventSubClient] - [SubscriptionManager] Added extra sub: " + missingSubscription.Type);
                    }
                }
            }
            return true;
        }

        public async Task ClearAsync(string clientId, string accessToken, string userId, ILogger logger, CancellationTokenSource clSource)
        {
            if (clientId == null)
            {
                throw new ArgumentNullException(nameof(clientId));
            }
            if (accessToken == null)
            {
                throw new ArgumentNullException(nameof(accessToken));
            }
            var allSubscriptions = await ApiTryGetAllSubscriptionsAsync(clientId, accessToken, userId, clSource, logger, StatusProvider.SubscriptionStatus.Empty);
            if (allSubscriptions is null)
            {
                return;
            }
            foreach (var getSubscriptionsResponse in allSubscriptions)
            {
                if (getSubscriptionsResponse is null || getSubscriptionsResponse.Data is null)
                {
                    logger.LogInformation("[EventSubClient] - [SubscriptionManager] Retrieved null Subscription Response");
                    continue;
                }

                foreach (var subscription in getSubscriptionsResponse.Data)
                {

                    if (subscription is null)
                    {
                        logger.LogInformation("[EventSubClient] - [SubscriptionManager] Retrieved null Subscription");
                        continue;
                    }

                    if (!await ApiTryUnSubscribeAsync(clientId, accessToken, subscription.Id, userId, logger, clSource))
                    {
                        logger.LogWarningDetails("[EventSubClient] - [SubscriptionManager] Failed to unsubscribe during clear", subscription);
                        continue;
                    }
                    logger.LogInformation("[EventSubClient] - [SubscriptionManager] Sub cleared: " + subscription.Type);
                }
            }
        }


        public Task<bool> ApiTrySubscribeAsync(string clientId, string accessToken, CreateSubscriptionRequest create, string userId, ILogger logger, CancellationTokenSource clSource)
        {
            Task<bool> TrySubscribeAsync() => TwitchApi.SubscribeAsync(clientId, accessToken, create, clSource, logger);
            return TryFuncAsync(TrySubscribeAsync, logger, userId);
        }

        private Task<bool> ApiTryUnSubscribeAsync(string clientId, string accessToken, string subId, string userId, ILogger logger, CancellationTokenSource clSource)
        {
            Task<bool> TryUnSubscribeAsync() => TwitchApi.UnSubscribeAsync(clientId, accessToken, subId, clSource, logger);
            return TryFuncAsync(TryUnSubscribeAsync, logger, userId);
        }

        private Task<List<GetSubscriptionsResponse>?> ApiTryGetAllSubscriptionsAsync(string clientId, string accessToken, string userId, CancellationTokenSource clSource, ILogger logger, StatusProvider.SubscriptionStatus statusSelector)
        {
            Task<List<GetSubscriptionsResponse>> TryGetAllSubscriptionsAsync() => TwitchApi.GetAllSubscriptionsAsync(clientId, accessToken, clSource, logger, statusSelector);
            return TryFuncAsync(TryGetAllSubscriptionsAsync, logger, userId);
        }


        /// <summary>
        /// This should catch any AccessToken exception and calls outside for changes.
        /// Then it calls function again.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="apiCallAction"></param>
        /// <returns></returns>
        private async Task<TType?> TryFuncAsync<TType>(Func<Task<TType>> apiCallAction, ILogger logger, string UserId)
        {
            try
            {
                return await apiCallAction();
            }
            catch (InvalidAccessTokenException ex)
            {
                ex.SourceUserId = UserId;
                //procedure must run UpdateOnFly function for proper change
                await OnRefreshTokenRequestAsync.TryInvoke(this, ex);
                logger.LogInformationDetails("[EventSubClient] - [SubscriptionManager] Invalid Access token detected, requesting change.", ex);
            }
            catch (TaskCanceledException)
            {
                //its alright, move on
            }
            catch (Exception ex)
            {
                logger.LogInformationDetails("[EventSubClient] - [SubscriptionManager] Api call failed due to:", ex);
            }
            //This is expected behavior. If we get null or false, we handle it in higher part of function
            logger.LogWarningDetails("[EventSubClient] - [SubscriptionManager] Try Func Async returned Default value.", apiCallAction.Method.Name);
            return default;
        }
    }
}
