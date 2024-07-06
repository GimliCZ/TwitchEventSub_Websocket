using Twitch.EventSub.API.Models;
using Twitch.EventSub.Interfaces;

namespace Twitch.EventSub
{
    public interface IEventSubClient
    {
        IEventProvider? this[string key] { get; }
        Task<bool> AddUserAsync(string userId, string accessToken, List<SubscriptionType> listOfSubs);
        Task<bool> DeleteUserAsync(string userId);
        IEventProvider? GetUserEventProvider(string userId);
        bool IsConnected(string userId);
        Task<bool> StartAsync(string userId);
        Task<bool> StopAsync(string userId);
        bool UpdateUser(string userId, string accessToken, List<SubscriptionType> listOfSubs);
    }
}