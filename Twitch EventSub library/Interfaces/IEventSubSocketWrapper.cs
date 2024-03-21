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

        event AsyncEventHandler<string?> OnRawMessageRecievedAsync;
        Task<bool> ConnectAsync(string connectUrl);
        Task DisconnectAsync();
    }
}
