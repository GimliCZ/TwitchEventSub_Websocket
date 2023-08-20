using Microsoft.VisualBasic.CompilerServices;
using Twitch.EventSub.API.Models;
using Twitch.EventSub.CoreFunctions;

namespace Twitch.EventSub.Interfaces
{
    public interface IEventSubscriptionManager
    {
        event AsyncEventHandler<InvalidAccessTokenException> OnRefreshTokenRequestAsync;

        /// <summary>
        /// This function serves for Connect and Reconnect cases
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="accessToken"></param>
        /// <param name="sessionId"></param>
        /// <param name="requestedSubscriptions"></param>
        /// <returns></returns>
        Task SetupAsync(string? clientId, string? accessToken, string? sessionId, List<CreateSubscriptionRequest>? requestedSubscriptions);

        /// <summary>
        /// This Function servers for handeling AccessToken changes ONLY
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="accessToken"></param>
        /// <param name="requestedSubscriptions"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Setup was not run</exception>
        /// <exception cref="Exception">May crash, if dupes found in list</exception>
        Task UpdateOnFlyAsync(string? clientId, string? accessToken, List<CreateSubscriptionRequest>? requestedSubscriptions);

        /// <summary>
        /// Creates repeated cycle for checking subscriptons. Even tho we should know about every change in
        /// subs revoke it serves as sanity check.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IncompleteInitialization">Setup was not run</exception>
        /// <exception cref="Exception">May crash, if dupes found in list</exception>
        /// <exception cref="ArgumentNullException">May crash for check sub has no valid values to run on</exception>
        void Start();

        Task RevocationResolverAsync(Messages.RevocationMessage.WebSocketRevocationMessage e);

        /// <summary>
        /// Stops repeated checking
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}
