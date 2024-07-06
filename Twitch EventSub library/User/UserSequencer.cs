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
        private AsyncAutoResetEvent _awaitRefresh = new(false);
        private SubscriptionManager _subscriptionManager;
        private Timer _managerTimer;

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
            _logger.LogDebug("[UserSequencer] Initialized with UserId: {UserId}, ClientId: {ClientId}", id, clientId);
            _managerTimer = new Timer(_ => OnManagerTimerEnlapsedAsync(), null, Timeout.Infinite, Timeout.Infinite);
        }

        private async void OnManagerTimerEnlapsedAsync()
        {
            await StateMachine.FireAsync(UserActions.RunningProceed);
        }

        protected override async Task RunHandshakeAsync()
        {
            _logger.LogDebug("[RunHandshakeAsync] Starting handshake for UserId: {UserId}", UserId);
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
                    _logger.LogDebug("[RunHandshakeAsync] Handshake success for UserId: {UserId}", UserId);
                    await StateMachine.FireAsync(UserActions.HandShakeSuccess);
                }
                else
                {
                    _logger.LogDebug("[RunHandshakeAsync] Handshake failed for UserId: {UserId}", UserId);
                    await StateMachine.FireAsync(UserActions.HandShakeFail);
                }
            }
        }

        private async Task OnRefreshTokenRequestAsync(object sender, InvalidAccessTokenException e)
        {
            _logger.LogDebug($"RefreshToken request: {e}");
            if (UserId != e.SourceUserId)
            {
                _logger.LogError("[OnRefreshTokenRequestAsync] SourceUserId does not match UserId: {UserId}", UserId);
                return;
            }
            if (e is null) 
            {
                _logger.LogError("[OnRefreshTokenRequestAsync] Got Null Invalid Access Token Exception");
            }
            LastAccessViolationException = e;
            _logger.LogDebug("[OnRefreshTokenRequestAsync] InvalidAccessTokenException received for UserId: {UserId}, State: {State}", UserId, StateMachine.State);
            switch (StateMachine.State)
            {
                case UserState.Running:
                    _logger.LogDebug($"[OnRefreshTokenRequestAsync] Invoking Running access token renew procedure {e}");
                    await StateMachine.FireAsync(UserActions.RunningAccessFail);
                    break;
                case UserState.HandShake:
                    _logger.LogDebug($"[OnRefreshTokenRequestAsync] Invoking Handshake access token renew procedure {e}");
                    await StateMachine.FireAsync(UserActions.HandShakeAccessFail);
                    break;
                case UserState.InicialAccessTest:
                    _logger.LogDebug($"[OnRefreshTokenRequestAsync] Invoking Test access token renew procedure {e}");
                    await StateMachine.FireAsync(UserActions.AccessFailed);
                    break;
                default:
                    _logger.LogError("[OnRefreshTokenRequestAsync] Unexpected state: {State}", StateMachine.State);
                    throw new InvalidOperationException("[EventSubClient] - [UserSequencer] OnRefreshTokenRequestAsync went into unknown state");
            };
            _awaitRefresh.Set();
        }

        protected override async Task AwaitWelcomeMessageAsync()
        {
            _logger.LogDebug("[AwaitWelcomeMessageAsync] Awaiting welcome message for UserId: {UserId}", UserId);
            try
            {
                using (var cls = new CancellationTokenSource(1000))
                {
                    await _awaitMessage.WaitAsync(cls.Token);
                    _logger.LogDebug("[AwaitWelcomeMessageAsync] Welcome message received for UserId: {UserId}", UserId);
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
                _logger.LogDebug("[RunManagerAsync] Running manager for UserId: {UserId}", UserId);
                _managerTimer.Change(Timeout.Infinite, Timeout.Infinite);
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
                    _logger.LogDebug("[RunManagerAsync] Manager check successful for UserId: {UserId}", UserId);
                    await StateMachine.FireAsync(UserActions.RunningAwait);
                }
                else
                {
                    _logger.LogDebug("[RunManagerAsync] Manager check failed for UserId: {UserId}", UserId);
                    await StateMachine.FireAsync(UserActions.Fail);
                }


            }
        }

        protected override async Task AwaitManagerAsync()
        {
            _managerTimer.Change(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
        }

        protected async Task StopManagerAsync()
        {
            _managerTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _logger.LogDebug("[StopManagerAsync] Stopping manager for UserId: {UserId}", UserId);
            await ManagerCancelationSource.CancelAsync();
        }


        protected override async Task RunWebsocketAsync()
        {
            _logger.LogDebug("[RunWebsocketAsync] Running WebSocket for UserId: {UserId}", UserId);
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
                _logger.LogDebug("[RunWebsocketAsync] WebSocket started successfully for UserId: {UserId}", UserId);
                await StateMachine.FireAsync(UserActions.WebsocketSuccess);
            }
            else
            {
                _logger.LogDebug("[RunWebsocketAsync] WebSocket failed to start for UserId: {UserId}", UserId);
                await StateMachine.FireAsync(UserActions.WebsocketFail);
            }
        }

        private async Task OnServerSideTerminationAsync(UserSequencer userSequencer, DisconnectionInfo disconnectInfo)
        {
            _watchdog.Stop();
            
            if (StateMachine.State == UserState.Stoping) 
            {
                _logger.LogInformation("[EventSubClient] - [UserSequencer] Socket correctly disconnected");
                return;
            }
            _logger.LogErrorDetails("[EventSubClient] - [UserSequencer] Socket Disconnected from outside", disconnectInfo);
            await StateMachine.FireAsync(UserActions.WebsocketFail);
        }

        private async Task SocketOnMessageReceivedAsync(UserSequencer userSequencer, string? text)
        {
            _logger.LogDebug("[SocketOnMessageReceivedAsync] Message received: {Text}", text);
            if (!string.IsNullOrEmpty(text))
            {
                await ParseWebSocketMessageAsync(text);
            }
        }


        private async Task<Task> ParseWebSocketMessageAsync(string e)
        {
            _logger.LogDebug("[ParseWebSocketMessageAsync] Parsing message: {Message}", e);
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
                    _logger.LogDebug("[ParseWebSocketMessageAsync] Duplicate or outdated message: {MessageId}", message.Metadata.MessageId);
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
                _logger.LogDebug("[InicialAccessTokenAsync] Validating initial access token for UserId: {UserId}", UserId);
                using (CancellationTokenSource cts = new CancellationTokenSource(10000))
                {
                    
                    var validationResult = await _subscriptionManager.ApiTryValidateAsync(AccessToken,UserId, _logger, cts);
                    if (validationResult)
                    {
                        _logger.LogDebug("[InicialAccessTokenAsync] Initial access token validated for UserId: {UserId}", UserId);
                        await StateMachine.FireAsync(UserActions.AccessSuccess);
                    }
                    else
                    {
                        _logger.LogDebug("[InicialAccessTokenAsync] Initial access token validation failed for UserId: {UserId}", UserId);
                        await StateMachine.FireAsync(UserActions.AccessFailed);
                    }
                }
            }

        }

        protected override async Task NewAccessTokenRequestAsync()
        {
            {
                _logger.LogDebug("[NewAccessTokenRequestAsync] Requesting new access token for UserId: {UserId}", UserId);
                await _awaitRefresh.WaitAsync();
                if (LastAccessViolationException != null)
                {
                    var invalidToken = AccessToken;
                    await AccessTokenRequestedEvent.TryInvoke(this, LastAccessViolationException);
                    _logger.LogErrorDetails("[EventSubClient] - [UserSequencer] Directly edit current object AccessToken", LastAccessViolationException);
                    var NewToken = AccessToken;
                    if (invalidToken == NewToken)
                    {
                        _logger.LogDebug("[NewAccessTokenRequestAsync] New token is the same as the invalid token for UserId: {UserId}", UserId);
                        await StateMachine.FireAsync(UserActions.AwaitNewTokenFailed);
                    }
                    switch (StateMachine.State)
                    {
                        case UserState.AwaitNewTokenAfterFailedTest:
                            _logger.LogDebug($"[NewAccessTokenRequestAsync] Returning to Test after Access Token renew {invalidToken}");
                            await StateMachine.FireAsync(UserActions.NewTokenProvidedReturnToInicialTest);
                            break;
                        case UserState.AwaitNewTokenAfterFailedHandShake:
                            _logger.LogDebug($"[NewAccessTokenRequestAsync] Returning to Handshake after Access Token renew {invalidToken}");
                            await StateMachine.FireAsync(UserActions.NewTokenProvidedReturnToHandShake);
                            break;
                        case UserState.AwaitNewTokenAfterFailedRun:
                            _logger.LogDebug($"[NewAccessTokenRequestAsync] Returning to Run after Access Token renew {invalidToken}");
                            await StateMachine.FireAsync(UserActions.NewTokenProvidedReturnToRunning);
                            break;
                        default:
                            _logger.LogError("[NewAccessTokenRequestAsync] Unexpected state: {State}", StateMachine.State);
                            throw new InvalidOperationException("NewAccessTokenRequest run into invalid state");
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
            _logger.LogDebug("[WelcomeMessageProcessingAsync] Processing welcome message for UserId: {UserId}", UserId);
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
            _logger.LogDebug("[ReconnectMessageProcessingAsync] Processing reconnect message for UserId: {UserId}", UserId);
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
            _logger.LogDebug("[RevocationMessageProcessingAsync] Processing revocation message for UserId: {UserId}", UserId);
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
            _logger.LogDebug("[PingMessageProcessingAsync] Ping message detected");
            Socket.Send("Pong");
            return Task.CompletedTask;
        }

        private Task KeepAliveMessageProcessingAsync()
        {
            _logger.LogDebug("[KeepAliveMessageProcessingAsync] KeepAlive message detected");
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
            _logger.LogDebug("[StopProcedureAsync] Stopping procedure for UserId: {UserId}", UserId);
            await StopManagerAsync();
            using (var cls = new CancellationTokenSource(10000))
            {
                await _subscriptionManager.ClearAsync(ClientId, AccessToken, UserId, _logger, cls);
            }
            _watchdog.Stop();
            await Socket.Stop(WebSocketCloseStatus.NormalClosure, "Closing");
            await StateMachine.FireAsync(UserActions.Dispose);
        }

        protected override async Task FailProcedureAsync()
        {
            _logger.LogDebug("[FailProcedureAsync] Failing procedure for UserId: {UserId}", UserId);
            await StateMachine.FireAsync(UserActions.Dispose);
        }
    }
}
