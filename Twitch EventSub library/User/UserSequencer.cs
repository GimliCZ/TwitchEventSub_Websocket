using System.Net.WebSockets;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using Twitch.EventSub.API;
using Twitch.EventSub.API.Models;
using Twitch.EventSub.CoreFunctions;
using Twitch.EventSub.Messages;
using Twitch.EventSub.Messages.KeepAliveMessage;
using Twitch.EventSub.Messages.NotificationMessage;
using Twitch.EventSub.Messages.PingMessage;
using Twitch.EventSub.Messages.ReconnectMessage;
using Twitch.EventSub.Messages.RevocationMessage;
using Twitch.EventSub.Messages.WelcomeMessage;
using Websocket.Client;

namespace Twitch.EventSub.User
{
    public class UserSequencer : UserBase
    {
        private ILogger _logger;
        private ReplayProtection _replayProtection;
        private Watchdog _watchdog;
        private AsyncAutoResetEvent _awaitMessage = new(false);
        private SubscriptionManager _subscriptionManager;

        public event CoreFunctions.AsyncEventHandler<string?> OnRawMessageRecievedAsync;
        public event CoreFunctions.AsyncEventHandler<string?> OnOutsideDisconnectAsync;
        public event CoreFunctions.AsyncEventHandler<InvalidAccessTokenException> AccessTokenRequestedEvent;
        public event CoreFunctions.AsyncEventHandler<WebSocketNotificationPayload> OnNotificationMessageAsync;
        public UserSequencer(string id, string access, List<CreateSubscriptionRequest> requestedSubscriptions, string clientId, ILogger logger) : base(id, access, requestedSubscriptions)
        {
            _logger = logger;
            ClientId = clientId;
            _watchdog = new Watchdog(logger);
            _replayProtection = new ReplayProtection(10);
            _subscriptionManager = new SubscriptionManager();
        }

        protected override async Task RunHandshakeAsync()
        {
            _subscriptionManager.OnRefreshTokenRequestAsync -= OnRefreshTokenRequestAsync;
            _subscriptionManager.OnRefreshTokenRequestAsync += OnRefreshTokenRequestAsync;
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                var checkOk = await _subscriptionManager.RunCheckAsync(
                    UserId,
                    RequestedSubscriptions,
                    ClientId,
                    AccessToken,
                    SessionId,
                    cts,
                    _logger
                    );

                if (checkOk)
                {
                    await StateMachine.FireAsync(UserActions.HandShakeSuccess);
                }
                else
                {
                    await StateMachine.FireAsync(UserActions.HandShakeFail);
                }
            }
        }

        private async Task OnRefreshTokenRequestAsync(object sender, InvalidAccessTokenException e)
        {
            if (UserId != e.SourceUserId)
            {
                return;
            }
            LastAccessViolationException = e;
            switch (StateMachine.State)
            {
                case UserState.Running:
                    await StateMachine.FireAsync(UserActions.RunningAccessFail);
                    break;
                case UserState.HandShake:
                    await StateMachine.FireAsync(UserActions.HandShakeAccessFail);
                    break;
                case UserState.InicialAccessTest:
                    await StateMachine.FireAsync(UserActions.AccessFailed);
                    break;
                default: throw new InvalidOperationException("[EventSubClient] - [UserSequencer] OnRefreshTokenRequestAsync went into unknown state");
            };
        }

        protected override async Task AwaitWelcomeMessageAsync()
        {
            try
            {
                using (var cls = new CancellationTokenSource(1000))
                {
                    await _awaitMessage.WaitAsync(cls.Token);
                    await StateMachine.FireAsync(UserActions.WelcomeMessageSuccess);
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorDetails("[EventSubClient] - [UserSequencer] Welcome message didn't come in time.", Socket, DateTime.Now);
                await StateMachine.FireAsync(UserActions.WelcomeMessageFail);
            }
        }

        protected override async Task RunManagerAsync()
        {
            {
                ManagerCancelationSource = new CancellationTokenSource();
                var checkOk = await _subscriptionManager.RunCheckAsync(
                    UserId,
                    RequestedSubscriptions,
                    ClientId,
                    AccessToken,
                    SessionId,
                    ManagerCancelationSource,
                    _logger
                    );
                if (checkOk)
                {
                    //repeat after 30 minutes
                    await StateMachine.FireAsync(UserActions.RunningAwait);
                }
                else
                {
                    await StateMachine.FireAsync(UserActions.Fail);
                }


            }
        }

        protected override async Task AwaitManagerAsync()
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(30), ManagerCancelationSource.Token).ConfigureAwait(false);
            }
            finally
            {
                await StateMachine.FireAsync(UserActions.RunningProceed);
            }
        }

        protected override async Task StopManagerAsync()
        {
            await ManagerCancelationSource.CancelAsync();
        }


        protected override async Task RunWebsocketAsync()
        {
            if (Socket.IsRunning)
            {
                _logger.LogInformation("[EventSubClient] - [UserSequencer] Socket already active");
                return;
            }
            Socket = new WebsocketClient(Url ?? new Uri(DefaultWebSocketUrl));
            Socket.MessageReceived.Select(msg => Observable.FromAsync(() => SocketOnMessageReceivedAsync(this, msg.Text))).Concat().Subscribe();
            Socket.DisconnectionHappened.Select(disconnectInfo => Observable.FromAsync(() => OnServerSideTerminationAsync(this, disconnectInfo))).Concat().Subscribe();
            await Socket.Start();
            if (Socket.IsRunning)
            {
                await StateMachine.FireAsync(UserActions.WebsocketSuccess);
            }
            else
            {
                await StateMachine.FireAsync(UserActions.WebsocketFail);
            }
        }

        private async Task OnServerSideTerminationAsync(UserSequencer userSequencer, DisconnectionInfo disconnectInfo)
        {
            _logger.LogErrorDetails("[EventSubClient] - [UserSequencer] Socket Disconnected from outside", disconnectInfo);
            await StateMachine.FireAsync(UserActions.WebsocketFail);
        }

        private async Task SocketOnMessageReceivedAsync(UserSequencer userSequencer, string? text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                await ParseWebSocketMessageAsync(text);
            }
        }


        private async Task<Task> ParseWebSocketMessageAsync(string e)
        {
            try
            {
                WebSocketMessage message;
                await OnRawMessageRecievedAsync(this, e);
                try
                {
                    message = await MessageProcessing.DeserializeMessageAsync(e);
                }
                catch (JsonException ex)
                {
                    // Log the parsing error and return immediately
                    _logger.LogErrorDetails("[EventSubClient] - [UserSequencer] Error while parsing WebSocket message: ", e, ex);
                    return Task.CompletedTask;
                }


                if (_replayProtection.IsDuplicate(message.Metadata.MessageId) ||
                    !_replayProtection.IsUpToDate(message.Metadata.MessageTimestamp))
                {
                    return Task.CompletedTask;
                }

                return message switch
                {
                    WebSocketWelcomeMessage welcomeMessage => WelcomeMessageProcessingAsync(welcomeMessage),
                    WebSocketKeepAliveMessage => KeepAliveMessageProcessingAsync(),
                    WebSocketPingMessage => PingMessageProcessingAsync(),
                    WebSocketNotificationMessage notificationMessage => NotificationMessageProcessingAsync(notificationMessage),
                    WebSocketReconnectMessage reconnectMessage => ReconnectMessageProcessingAsync(reconnectMessage),
                    WebSocketRevocationMessage revocationMessage => RevocationMessageProcessingAsync(revocationMessage),
                    _ => throw new JsonSerializationException($"Unsupported message_type: {message}")
                };
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions and log them
                _logger.LogErrorDetails("[EventSubClient] - [UserSequencer] Unexpected error while processing WebSocket message: ", ex);
                return Task.CompletedTask;
            }
        }

        protected override async Task InicialAccessTokenAsync()
        {
            {
                using (CancellationTokenSource cts = new CancellationTokenSource(1000))
                {
                    if (await TwitchApi.ValidateTokenAsync(AccessToken, cts, _logger))
                    {
                        await StateMachine.FireAsync(UserActions.AccessSuccess);
                    }
                    else
                    {
                        await StateMachine.FireAsync(UserActions.AccessFailed);
                    }
                }
            }

        }

        protected override async Task NewAccessTokenRequestAsync()
        {
            {
                if (LastAccessViolationException != null)
                {
                    var invalidToken = AccessToken;
                    await AccessTokenRequestedEvent.TryInvoke(this, LastAccessViolationException);
                    _logger.LogErrorDetails("[EventSubClient] - [UserSequencer] Directly edit current object AccessToken", LastAccessViolationException);
                    var NewToken = AccessToken;
                    if (invalidToken == NewToken)
                    {
                        await StateMachine.FireAsync(UserActions.AwaitNewTokenFailed);
                    }
                    switch (StateMachine.State)
                    {
                        case UserState.AwaitNewTokenAfterFailedTest:
                            await StateMachine.FireAsync(UserActions.NewTokenProvidedReturnToInicialTest);
                            break;
                        case UserState.AwaitNewTokenAfterFailedHandShake:
                            await StateMachine.FireAsync(UserActions.NewTokenProvidedReturnToHandShake);
                            break;
                        case UserState.AwaitNewTokenAfterFailedRun:
                            await StateMachine.FireAsync(UserActions.NewTokenProvidedReturnToRunning);
                            break;
                        default: throw new InvalidOperationException("NewAccessTokenRequest run into invalid state");
                    };
                }
                else
                {
                    _logger.LogErrorDetails("[EventSubClient] - [UserSequencer] Request for New Access token didnt contain valid exception", LastAccessViolationException);
                    await StateMachine.FireAsync(UserActions.AwaitNewTokenFailed);
                }
            }
        }

        private async Task WelcomeMessageProcessingAsync(WebSocketWelcomeMessage message)
        {
            if (message?.Payload?.Session?.Id != null)
            {
                SessionId = message.Payload.Session.Id;
                _logger.LogDebugDetails("[EventSubClient] - [UserSequencer] Welcome message detected, Session captured", message, DateTime.Now);
            }
            else
            {
                _logger.LogErrorDetails("[EventSubClient] - [UserSequencer] Session key invalid", message, DateTime.Now);
            }

            // keep alive in sec + 10% tolerance
            if (message?.Payload?.Session.KeepAliveTimeoutSeconds != null)
            {
                var _keepAlive = (message.Payload.Session.KeepAliveTimeoutSeconds.Value * 1000 + 100);
                _awaitMessage.Set();
                _watchdog.Start(_keepAlive);
                _logger.LogDebugDetails("[EventSubClient] - [UserSequencer] Welcome message proccesed", message, DateTime.Now);
            }
            else
            {
                _logger.LogErrorDetails("[EventSubClient] - [UserSequencer] Welcome message detected, but did not contain keep alive.", message, DateTime.Now);
            }

        }

        private async Task NotificationMessageProcessingAsync(WebSocketNotificationMessage message)
        {
            _watchdog.Reset();
            if (message.Payload != null)
            {
                await OnNotificationMessageAsync(this, message.Payload);
            }
            _logger.LogDebugDetails("[EventSubClient] - [UserSequencer] Notification message detected", message, DateTime.Now);
        }

        private async Task ReconnectMessageProcessingAsync(WebSocketReconnectMessage message)
        {
            await StateMachine.FireAsync(UserActions.ReconnectRequested);
            _watchdog.Stop();

            if (message?.Payload?.Session.ReconnectUrl != null)
            {
                if (Uri.TryCreate(message.Payload.Session.ReconnectUrl, new UriCreationOptions() { DangerousDisablePathAndQueryCanonicalization = false }, out var Url))
                {
                    Socket.Url = Url;
                    await Socket.ReconnectOrFail();
                    Socket.MessageReceived.Select(msg => Observable.FromAsync(() => SocketOnMessageReceivedAsync(this, msg.Text))).Concat().Subscribe();
                    Socket.DisconnectionHappened.Select(disconnectInfo => Observable.FromAsync(() => OnServerSideTerminationAsync(this, disconnectInfo))).Concat().Subscribe();
                    if (!Socket.IsRunning)
                    {
                        _logger.LogInformationDetails("[EventSubClient] - [UserSequencer] connection lost during reconnect", message, Socket);
                        await StateMachine.FireAsync(UserActions.ReconnectFail);
                        return;
                    }
                }
                else
                {
                    _logger.LogInformationDetails("[EventSubClient] - [UserSequencer] Didn't recieve valid Url during Reconnect", message, Socket);
                    await StateMachine.FireAsync(UserActions.ReconnectFail);
                    return;
                }

            }
            if (message.Payload.Session.KeepAliveTimeoutSeconds.HasValue)
            {
                _watchdog.Start(message.Payload.Session.KeepAliveTimeoutSeconds.Value);
            }
            else
            {
                _watchdog.Start(30);
                _logger.LogInformationDetails("[EventSubClient] - [UserSequencer] Reconnect keep alive value not provided, trying to insert 30s", message, Socket);
            }

            if (!string.IsNullOrEmpty(message?.Payload?.Session.Id))
            {
                SessionId = message.Payload.Session.Id;
            }
            else
            {
                _logger.LogInformationDetails("[EventSubClient] - [UserSequencer] Provided invalid session. Terminiting", message, Socket);
                await StateMachine.FireAsync(UserActions.ReconnectFail);
            }
            await StateMachine.FireAsync(UserActions.ReconnectSuccess);
        }

        private async Task RevocationMessageProcessingAsync(WebSocketRevocationMessage e)
        {

            if (RequestedSubscriptions == null || ClientId == null || AccessToken == null)
            {
                _logger.LogInformation("[EventSubClient] - [SubscriptionManager] Revocation Resolver got subscriptions, clientId or accessToken as Null");
                return;
            }
            foreach (var sub in RequestedSubscriptions.Where(sub => sub.Type == e?.Payload?.Type && sub.Version == e?.Payload?.Version))
            {
                using (var cls = new CancellationTokenSource(1000))
                {
                    if (!await _subscriptionManager.ApiTrySubscribeAsync(ClientId, AccessToken, sub, UserId, _logger, cls))
                    {
                        _logger.LogInformation("[EventSubClient] - [SubscriptionManager] Failed to subscribe subscription during revocation");
                        return;
                    }
                }
                _logger.LogInformationDetails("[EventSubClient] - [SubscriptionManager] Refreshed sub due revocation: " + sub.Type + "caused by ", e);
            }
            _logger.LogDebugDetails("[EventSubClient] - [UserSequencer] Revocation message detected", e);
        }

        private Task PingMessageProcessingAsync()
        {
            Socket.Send("Pong");
            return Task.CompletedTask;
        }

        private Task KeepAliveMessageProcessingAsync()
        {
            _watchdog.Reset();
            _logger.LogDebugDetails("[EventSubClient] - [UserSequencer] KeepAlive message detected", DateTime.Now);
            return Task.CompletedTask;
        }

        private async Task OnWatchdogTimeoutAsync(object sender, string e)
        {
            await Socket.Stop(WebSocketCloseStatus.NormalClosure, "Server didn't respond in time");
            _logger.LogWarningDetails("[EventSubClient] - [UserSequencer] Server didn't respond in time", sender, e, DateTime.Now);
            if (OnOutsideDisconnectAsync != null)
            {
                await OnOutsideDisconnectAsync.TryInvoke(this, e);
            }
            Socket.Dispose();
            await StateMachine.FireAsync(UserActions.Fail);
        }

        protected override async Task StopProcedureAsync()
        {
            using (var cls = new CancellationTokenSource(1000))
            {
                await _subscriptionManager.ClearAsync(ClientId, AccessToken, UserId, _logger, cls);
            }
            _watchdog.Stop();
            await Socket.Stop(WebSocketCloseStatus.NormalClosure, "Closing");
            await StateMachine.FireAsync(UserActions.Dispose);
        }

        protected override async Task FailProcedureAsync()
        {
            await StateMachine.FireAsync(UserActions.Dispose);
        }
    }
}
