using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Twitch.EventSub.API.Models;
using Twitch.EventSub.CoreFunctions;
using Twitch.EventSub.Interfaces;
using Twitch.EventSub.User;

namespace Twitch.EventSub
{
    public class EventSubClient : IEventSubClient
    {
        private readonly ILogger _logger;
        private readonly string _clientId;
        public IEventProvider? this[string key] => GetUserEventProvider(key);
        private readonly ConcurrentDictionary<string, EventProvider> _eventDictionary;

        public EventSubClient(string clientId, ILogger<EventSubClient> logger)
        {
            _clientId = clientId;
            _eventDictionary = new ConcurrentDictionary<string, EventProvider>();
            _logger = logger;
            _logger.LogDebug("EventSubClient instantiated with clientId: {ClientId}", clientId);
        }
        public IEventProvider? GetUserEventProvider(string userId)
        {
            _eventDictionary.TryGetValue(userId, out var provider);
            return provider;
        }
        public async Task<bool> AddUserAsync(
            string userId,
            string accessToken,
            List<SubscriptionType> listOfSubs)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogErrorDetails("AddUser Failed due null or empty key",  userId);
                return false;
            }
            if (_eventDictionary.ContainsKey(userId))
            {
                _logger.LogErrorDetails("AddUser Failed due key being already in dictionary", userId);
                return false;
            }

            var eventProvider = new EventProvider(userId, accessToken, listOfSubs, _clientId, _logger);
            _logger.LogDebug("Attempting to add user");
            return _eventDictionary.TryAdd(userId, eventProvider);
        }
        public bool UpdateUser(string userId, string accessToken, List<SubscriptionType> listOfSubs)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogErrorDetails("UpdateUser Failed due null or empty key", userId);
                return false;
            }
            if (_eventDictionary.TryGetValue(userId, out var eventProvider))
            {
                _logger.LogDebug("Attempting to update user");
                return eventProvider.Update(accessToken, listOfSubs);
            }
            else
            {
                _logger.LogErrorDetails("UpdateUser Failed due key being already in dictionary", userId);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            _logger.LogDebug("Attempting to delete user with userId: {UserId}", userId);
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("DeleteUser failed due to null or empty userId");
                return false;
            }
            if (_eventDictionary.TryGetValue(userId, out var sequencer))
            {
                await sequencer.StopAsync();
                if (_eventDictionary.TryRemove(userId, out _))
                {
                    return true;
                }
            }
            _logger.LogError("DeleteUser failed because userId does not exist: {UserId}", userId);
            return false;
        }

        public async Task<bool> StartAsync(string userId)
        {
            _logger.LogDebug("Attempting to start user with userId: {UserId}", userId);
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("StartAsync failed due to null or empty userId");
                return false;
            }
            _eventDictionary.TryGetValue(userId, out var provider);
            if (provider is null)
            {
                return false;
            }
            _logger.LogDebug("StartAsync succeeded for userId: {UserId}", userId);
            await provider.StartAsync();
            return true;
        }

        public Task<bool> StopAsync(string userId)
        {
            _logger.LogDebug("Attempting to stop user with userId: {UserId}", userId);

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("StopAsync failed due to null or empty userId");
                return Task.FromResult(false);
            }
            _eventDictionary.TryGetValue(userId, out var provider);
            if (provider is null)
            {
                return Task.FromResult(false);
            }
            _logger.LogDebug("StopAsync succeeded for userId: {UserId}", userId);
            return provider.StopAsync();
        }
        public bool IsConnected(string userId)
        {
            _logger.LogDebug("Checking if user is connected with userId: {UserId}", userId);
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("IsConnected check failed due to null or empty userId");
                return false;
            }
            _eventDictionary.TryGetValue(userId, out var provider);
            if (provider is null)
            {
                return false;
            }
            return provider.IsConnected;
        }
    }
}