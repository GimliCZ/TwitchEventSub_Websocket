using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Twitch.EventSub.API.Models;
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
            if (_eventDictionary.ContainsKey(userId))
            {
                return false;
            }

            var eventProvider = new EventProvider(userId, accessToken, listOfSubs, _clientId, _logger);
            await eventProvider.StartAsync();

            return _eventDictionary.TryAdd(userId, eventProvider);
        }
        public bool UpdateUser(string userId, string accessToken, List<SubscriptionType> listOfSubs)
        {

            if (_eventDictionary.TryGetValue(userId, out var eventProvider))
            {
                return eventProvider.Update(accessToken, listOfSubs);
            }
            else
            {
                return false;
            }
        }
        public async Task<bool> DeleteUserAsync(string userId)
        {
            if (_eventDictionary.TryGetValue(userId, out var sequencer))
            {
                await sequencer.StopAsync();
                if (_eventDictionary.TryRemove(userId, out _))
                {
                    return true;
                }
            }
            return false;
        }
    }
}