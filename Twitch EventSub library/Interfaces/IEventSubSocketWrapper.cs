using Twitch.EventSub.CoreFunctions;
using Twitch.EventSub.Messages.NotificationMessage;
using Twitch.EventSub.Messages.RevocationMessage;

namespace Twitch.EventSub.Interfaces
{
    public interface IEventSubSocketWrapper
    {
        event AsyncEventHandler<string?> OnRegisterSubscriptionsAsync;

        event AsyncEventHandler<WebSocketNotificationPayload> OnNotificationMessageAsync;

        event AsyncEventHandler<WebSocketRevocationMessage> OnRevocationMessageAsync;

        event AsyncEventHandler<string?> OnOutsideDisconnectAsync;
        Task<bool> ConnectAsync(string connectUrl);
        Task DisconnectAsync();
    }
}
