namespace Twitch.EventSub.API.Models
{
    public class StatusProvider
    {
        public enum SubscriptionStatus
        {
            Enabled,
            NotificationFailuresExceeded,
            AuthorizationRevoked,
            ModeratorRemoved,
            UserRemoved,
            VersionRemoved,
            WebsocketDisconnected,
            WebsocketFailedPingPong,
            WebsocketReceivedInboundTraffic,
            WebsocketConnectionUnused,
            WebsocketInternalError,
            WebsocketNetworkTimeout,
            WebsocketNetworkError,

            //My addition - describes all possible states
            Empty
        }

        public static string GetStatusString(SubscriptionStatus status)
        {
            return status switch
            {
                SubscriptionStatus.Enabled => "enabled",
                SubscriptionStatus.NotificationFailuresExceeded => "notification_failures_exceeded",
                SubscriptionStatus.AuthorizationRevoked => "authorization_revoked",
                SubscriptionStatus.ModeratorRemoved => "moderator_removed",
                SubscriptionStatus.UserRemoved => "user_removed",
                SubscriptionStatus.VersionRemoved => "version_removed",
                SubscriptionStatus.WebsocketDisconnected => "websocket_disconnected",
                SubscriptionStatus.WebsocketFailedPingPong => "websocket_failed_ping_pong",
                SubscriptionStatus.WebsocketReceivedInboundTraffic => "websocket_received_inbound_traffic",
                SubscriptionStatus.WebsocketConnectionUnused => "websocket_connection_unused",
                SubscriptionStatus.WebsocketInternalError => "websocket_internal_error",
                SubscriptionStatus.WebsocketNetworkTimeout => "websocket_network_timeout",
                SubscriptionStatus.WebsocketNetworkError => "websocket_network_error",
                SubscriptionStatus.Empty => string.Empty,
                _ => throw new ArgumentException("Invalid subscription status.")
            };
        }
    }
}