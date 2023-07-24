using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Twitch_EventSub_library.CoreFunctions;
using Twitch_EventSub_library.Messages;
using Twitch_EventSub_library.Messages.KeepAliveMessage;
using Twitch_EventSub_library.Messages.NotificationMessage;
using Twitch_EventSub_library.Messages.PingMessage;
using Twitch_EventSub_library.Messages.ReconnectMessage;
using Twitch_EventSub_library.Messages.RevocationMessage;
using Twitch_EventSub_library.Messages.WelcomeMessage;

namespace Twitch_EventSub_library
{
    public class EventSubSocketWrapper
    {
        private const string DefaultWebSocketUrl = "wss://eventsub.wss.twitch.tv/ws";
        private readonly GenericWebsocket _socket;
        private readonly ILogger<EventSubSocketWrapper> _logger;
        private readonly ReplayProtection _replayProtection;
        private bool _awaitForReconnect = false;
        private readonly Watchdog _watchdog;
        private string _sessionId;
        private int _keepAlive;
        private bool _connectionActive;

        public event AsyncEventHandler<string> OnRegisterSubscriptions;
        public event AsyncEventHandler<WebSocketNotificationPayload> OnNotificationMessage;
        public event AsyncEventHandler<WebSocketRevocationMessage> OnRevocationMessage;
        public event AsyncEventHandler<string> OnOutsideDisconnect;

        public EventSubSocketWrapper(ILogger<EventSubSocketWrapper> logger,
            ILogger<GenericWebsocket> socketLogger,
            ILogger<Watchdog> watchdogLogger,
            TimeSpan processingSpeed)
        {
            _socket = new GenericWebsocket(socketLogger, processingSpeed);
            _logger = logger;
            _socket.OnMessageReceived += Socket_OnMessageReceived;
            _socket.OnServerSideTerminationReasoning += Socket_OnServerSideTerminationReasoning;
            _replayProtection = new ReplayProtection(10);
            _watchdog = new Watchdog(watchdogLogger);
            _watchdog.WatchdogTimeout += OnWatchdogTimeout;
        }

        private async Task Socket_OnServerSideTerminationReasoning(object sender, string e)
        {
            _logger.LogInformation(e);
            _connectionActive = false;
            await OnOutsideDisconnect.Invoke(this, e);
        }

        public async Task<bool> ConnectAsync(string connectUrl = DefaultWebSocketUrl)
        {
            if (_connectionActive)
            {
                return true;
            }
            _connectionActive = await _socket.ConnectAsync(connectUrl);
            return _connectionActive;
        }

        public async Task DisconnectAsync()
        {
            if (!_connectionActive)
            {
                return;
            }

            await _socket.DisconnectAsync();
            _connectionActive = false;
        }

        private Task Socket_OnMessageReceived(object sender, string e)
        {
            WebSocketMessage? message = JsonConvert.DeserializeObject<WebSocketMessage>(e);

            if (message == null ||
                _replayProtection.IsDuplicate(message.Metadata.MessageId) ||
                !_replayProtection.IsUpToDate(message.Metadata.MessageTimestamp))
            {
                return Task.CompletedTask;
            }

            return message switch
            {
                WebSocketWelcomeMessage welcomeMessage => WelcomeMessageProcessing(welcomeMessage),
                WebSocketKeepAliveMessage keepAliveMessage => KeepAliveMessageProcessing(keepAliveMessage),
                WebSocketPingMessage pingMessage => PingMessageProcessing(pingMessage),
                WebSocketNotificationMessage notificationMessage => NotificationMessageProcessing(notificationMessage),
                WebSocketReconnectMessage reconnectMessage => ReconnectMessageProcessing(reconnectMessage),
                WebSocketRevocationMessage revocationMessage => RevocationMessageProcessing(revocationMessage),
                _ => throw new JsonSerializationException($"Unsupported message_type: {message}")
            };
        }

        private async Task WelcomeMessageProcessing(WebSocketWelcomeMessage message)
        {
            _sessionId = message.Payload.Session.Id;
            await OnRegisterSubscriptions.Invoke(this, _sessionId);
            // keep alive in sec + 10% tolerance
            _keepAlive = (message.Payload.Session.KeepAliveTimeoutSeconds * 1000 + 100);
            _watchdog.Start(_keepAlive);
        }

        private async Task NotificationMessageProcessing(WebSocketNotificationMessage message)
        {
           await OnNotificationMessage.Invoke(this,message.Payload);
        }

        private async Task ReconnectMessageProcessing(WebSocketReconnectMessage message)
        {
            _awaitForReconnect = true;
            _watchdog.Stop();
            await _socket.DisconnectAsync();
            if (message.Payload.Session.ReconnectUrl != null)
            {
                _connectionActive = await _socket.ConnectAsync(message.Payload.Session.ReconnectUrl);
                if (!_connectionActive)
                {
                    _logger.LogInformation("connection lost during reconnect");
                    return;
                }
            }
            _watchdog.Start(_keepAlive);
            _sessionId = message.Payload.Session.Id;
            await OnRegisterSubscriptions.Invoke(this, _sessionId);
            _awaitForReconnect = false;
        }

        private async Task RevocationMessageProcessing(WebSocketRevocationMessage message)
        {
           await OnRevocationMessage.Invoke(this, message);
        }

        private Task PingMessageProcessing(WebSocketPingMessage message)
        {
            if (_awaitForReconnect == false)
            {
                _socket.Send("Pong");
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }

        private Task KeepAliveMessageProcessing(WebSocketKeepAliveMessage message)
        {
            _watchdog.Reset();
            return Task.CompletedTask;
        }

        private async Task OnWatchdogTimeout(object sender, string e)
        {
            await _socket.DisconnectAsync();
            _logger.LogInformation("Server didn't respond in time");
            await OnOutsideDisconnect.Invoke(this, e);
            _connectionActive = false;
        }
    }

}
