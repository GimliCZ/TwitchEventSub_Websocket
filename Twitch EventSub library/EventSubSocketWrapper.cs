using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Twitch.EventSub.CoreFunctions;
using Twitch.EventSub.Interfaces;
using Twitch.EventSub.Library.CoreFunctions;
using Twitch.EventSub.Library.Messages.NotificationMessage.Events;
using Twitch.EventSub.Messages;
using Twitch.EventSub.Messages.KeepAliveMessage;
using Twitch.EventSub.Messages.NotificationMessage;
using Twitch.EventSub.Messages.NotificationMessage.Events;
using Twitch.EventSub.Messages.PingMessage;
using Twitch.EventSub.Messages.ReconnectMessage;
using Twitch.EventSub.Messages.RevocationMessage;
using Twitch.EventSub.Messages.SharedContents;
using Twitch.EventSub.Messages.WelcomeMessage;

namespace Twitch.EventSub
{
    public class EventSubSocketWrapper : IEventSubSocketWrapper
    {
        private const string DefaultWebSocketUrl = "wss://eventsub.wss.twitch.tv/ws";
        private readonly ILogger _logger;
        private readonly ReplayProtection _replayProtection;
        private bool _awaitForReconnect = false;
        private readonly Watchdog _watchdog;

        private string? SessionId { get; set; }

        private readonly GenericWebsocket _socket;

        private int _keepAlive;
        private bool _connectionActive;

        public event AsyncEventHandler<string?> OnRegisterSubscriptionsAsync;
        public event AsyncEventHandler<WebSocketNotificationPayload> OnNotificationMessageAsync;
        public event AsyncEventHandler<WebSocketRevocationMessage> OnRevocationMessageAsync;
        public event AsyncEventHandler<string?> OnOutsideDisconnectAsync;

        public EventSubSocketWrapper(ILogger logger,
            ILogger socketLogger,
            ILogger watchdogLogger,
            TimeSpan processingSpeed)
        {
            _socket = new GenericWebsocket(socketLogger, processingSpeed);
            _logger = logger;
            _socket.OnMessageReceivedAsync += SocketOnMessageReceivedAsync;
            _socket.OnServerSideTerminationAsync += OnServerSideTerminationAsync;
            _replayProtection = new ReplayProtection(10);
            _watchdog = new Watchdog(watchdogLogger);
            _watchdog.OnWatchdogTimeout += OnWatchdogTimeoutAsync;
        }

        private async Task OnServerSideTerminationAsync(object sender, string e)
        {
            _logger.LogInformation(e);
            _connectionActive = false;
            await OnOutsideDisconnectAsync.TryInvoke(this, e);
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
            _watchdog.Stop();
            await _socket.DisconnectAsync();
            _connectionActive = false;
        }

        private Task SocketOnMessageReceivedAsync(object sender, string e)
        {
            return ParseWebSocketMessageAsync(e);
        }

        private Task ParseWebSocketMessageAsync(string e)
        {
            try
            {
                WebSocketMessage message;
                try
                {
                    message = DeserializeMessage(e);
                }
                catch (JsonException ex)
                {
                    // Log the parsing error and return immediately
                    _logger.LogError("[EventSubClient] - [EventSubSocketWrapper] Error while parsing WebSocket message: " + ex.Message, ex);
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
                    WebSocketKeepAliveMessage => KeepAliveMessageProcessing(),
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
                _logger.LogError("[EventSubClient] - [EventSubSocketWrapper] Unexpected error while processing WebSocket message: " + ex.Message, ex);
                return Task.CompletedTask;
            }
        }

        private static WebSocketMessage DeserializeMessage(string message)
        {
            JObject jsonObject = JObject.Parse(message);

            if (!jsonObject.TryGetValue("metadata", out JToken? metadataToken) || !(metadataToken is JObject))
            {
                throw new JsonSerializationException("metadata is missing in the JSON object");
            }
            var metadata = metadataToken.ToObject<WebSocketMessageMetadata>();
            if (metadata == null)
            {
                throw new JsonSerializationException();
            }
            string messageType = metadata.MessageType;

            if (!jsonObject.TryGetValue("payload", out JToken? payloadToken) || !(payloadToken is JObject))
            {
                throw new JsonSerializationException("metadata is missing in the JSON object");
            }

            return messageType switch
            {
                "session_welcome" => new WebSocketWelcomeMessage()
                {
                    Metadata = metadata,
                    Payload = payloadToken.ToObject<WebSocketWelcomePayload>()
                },
                "notification" => new WebSocketNotificationMessage()
                {
                    Metadata = metadata,
                    Payload = CreateNotificationPayload(payloadToken)
                },
                "ping" => new WebSocketPingMessage()
                {
                    Metadata = metadata
                },
                "session_keepalive" => new WebSocketKeepAliveMessage()
                {
                    Metadata = metadata,
                },
                "session_reconnect" => new WebSocketReconnectMessage()
                {
                    Metadata = metadata,
                    Payload = payloadToken?.ToObject<WebSocketReconnectPayload>()
                },
                "revocation" => new WebSocketRevocationMessage()
                {
                    Metadata = metadata,
                    Payload = payloadToken?.ToObject<WebSocketSubscription>()
                },
                _ => throw new JsonSerializationException($"Unsupported message_type: {messageType}")
            };
        }

        private static WebSocketNotificationPayload CreateNotificationPayload(JToken payload)
        {
            var resultMessage = new WebSocketNotificationPayload
            {
                Subscription = payload["subscription"]?.ToObject<WebSocketSubscription>()
            };

            // Deserialize the event payload based on the event type
            switch (payload["subscription"]?["type"]?.ToObject<string>())
            {
                case "channel.update":
                    resultMessage.Event = payload["event"]?.ToObject<UpdateNotificationEvent>();
                    break;

                case "channel.follow":
                    resultMessage.Event = payload["event"]?.ToObject<FollowEvent>();
                    break;

                case "channel.subscribe":
                    resultMessage.Event = payload["event"]?.ToObject<SubscribeEvent>();
                    break;

                case "channel.subscription.end":
                    resultMessage.Event = payload["event"]?.ToObject<SubscribeEndEvent>();
                    break;

                case "channel.subscription.gift":
                    resultMessage.Event = payload["event"]?.ToObject<SubscriptionGiftEvent>();
                    break;

                case "channel.subscription.message":
                    resultMessage.Event = payload["event"]?.ToObject<SubscriptionMessageEvent>();
                    break;

                case "channel.cheer":
                    resultMessage.Event = payload["event"]?.ToObject<CheerEvent>();
                    break;

                case "channel.raid":
                    resultMessage.Event = payload["event"]?.ToObject<RaidEvent>();
                    break;

                case "channel.ban":
                    resultMessage.Event = payload["event"]?.ToObject<BanEvent>();
                    break;

                case "channel.unban":
                    resultMessage.Event = payload["event"]?.ToObject<UnBanEvent>();
                    break;

                case "channel.moderator.add":
                    resultMessage.Event = payload["event"]?.ToObject<ModeratorAddEvent>();
                    break;

                case "channel.moderator.remove":
                    resultMessage.Event = payload["event"]?.ToObject<ModeratorRemoveEvent>();
                    break;

                case "channel.guest_star_session.begin":
                    resultMessage.Event = payload["event"]?.ToObject<GuestStarSessionBeginEvent>();
                    break;

                case "channel.guest_star_session.end":
                    resultMessage.Event = payload["event"]?.ToObject<GuestStarSessionEndEvent>();
                    break;

                case "channel.guest_star_guest.update":
                    resultMessage.Event = payload["event"]?.ToObject<GuestStarGuestUpdateEvent>();
                    break;

                case "channel.guest_star_slot.update":
                    resultMessage.Event = payload["event"]?.ToObject<GuestStarSlotUpdateEvent>();
                    break;

                case "channel.guest_star_settings.update":
                    resultMessage.Event = payload["event"]?.ToObject<GuestStarSettingsUpdateEvent>();
                    break;

                case "channel.channel_points_custom_reward.add":
                    resultMessage.Event = payload["event"]?.ToObject<PointsCustomRewardAddEvent>();
                    break;

                case "channel.channel_points_custom_reward.update":
                    resultMessage.Event = payload["event"]?.ToObject<PointsCustomRewardUpdateEvent>();
                    break;

                case "channel.channel_points_custom_reward.remove":
                    resultMessage.Event = payload["event"]?.ToObject<PointsCustomRewardRemoveEvent>();
                    break;

                case "channel.channel_points_custom_reward_redemption.add":
                    resultMessage.Event = payload["event"]?.ToObject<PointsCustomRewardRedemptionAddEvent>();
                    break;

                case "channel.channel_points_custom_reward_redemption.update":
                    resultMessage.Event = payload["event"]?.ToObject<PointsCustomRewardRedemptionUpdateEvent>();
                    break;

                case "channel.poll.begin":
                    resultMessage.Event = payload["event"]?.ToObject<PollBeginEvent>();
                    break;

                case "channel.poll.progress":
                    resultMessage.Event = payload["event"]?.ToObject<PollProgressEvent>();
                    break;

                case "channel.poll.end":
                    resultMessage.Event = payload["event"]?.ToObject<PollEndEvent>();
                    break;

                case "channel.prediction.begin":
                    resultMessage.Event = payload["event"]?.ToObject<PredictionBeginEvent>();
                    break;

                case "channel.prediction.progress":
                    resultMessage.Event = payload["event"]?.ToObject<PredictionProgressEvent>();
                    break;

                case "channel.prediction.lock":
                    resultMessage.Event = payload["event"]?.ToObject<PredictionLockEvent>();
                    break;

                case "channel.prediction.end":
                    resultMessage.Event = payload["event"]?.ToObject<PredictionEndEvent>();
                    break;

                case "channel.charity_campaign.donate":
                    resultMessage.Event = payload["event"]?.ToObject<CharityDonationEvent>();
                    break;

                case "channel.charity_campaign.start":
                    resultMessage.Event = payload["event"]?.ToObject<CharityCampaignStartEvent>();
                    break;

                case "channel.charity_campaign.progress":
                    resultMessage.Event = payload["event"]?.ToObject<CharityCampaignProgressEvent>();
                    break;

                case "channel.charity_campaign.stop":
                    resultMessage.Event = payload["event"]?.ToObject<CharityCampaignStopEvent>();
                    break;

                case "channel.hype_train.begin":
                    resultMessage.Event = payload["event"]?.ToObject<HypeTrainBeginEvent>();
                    break;

                case "channel.hype_train.progress":
                    resultMessage.Event = payload["event"]?.ToObject<HypeTrainProgressEvent>();
                    break;

                case "channel.hype_train.end":
                    resultMessage.Event = payload["event"]?.ToObject<HypeTrainEndEvent>();
                    break;

                case "channel.shield_mode.begin":
                    resultMessage.Event = payload["event"]?.ToObject<ShieldModeBeginEvent>();
                    break;

                case "channel.shield_mode.end":
                    resultMessage.Event = payload["event"]?.ToObject<ShieldModeEndEvent>();
                    break;

                case "channel.shoutout.create":
                    resultMessage.Event = payload["event"]?.ToObject<ShoutoutCreateEvent>();
                    break;

                case "channel.shoutout.receive":
                    resultMessage.Event = payload["event"]?.ToObject<ShoutoutReceivedEvent>();
                    break;

                case "channel.goal.begin":
                    resultMessage.Event = payload["event"]?.ToObject<GoalBeginEvent>();
                    break;

                case "channel.goal.progress":
                    resultMessage.Event = payload["event"]?.ToObject<GoalProgressEvent>();
                    break;

                case "channel.goal.end":
                    resultMessage.Event = payload["event"]?.ToObject<GoalEndEvent>();
                    break;
                case "stream.online":
                    resultMessage.Event = payload["event"]?.ToObject<StreamOnlineEvent>();
                    break;

                case "stream.offline":
                    resultMessage.Event = payload["event"]?.ToObject<StreamOfflineEvent>();
                    break;

                default:
                    break;
            }
            return resultMessage;
        }

        private async Task WelcomeMessageProcessingAsync(WebSocketWelcomeMessage message)
        {
            SessionId = message?.Payload?.Session.Id;
            await OnRegisterSubscriptionsAsync.TryInvoke(this, SessionId);
            // keep alive in sec + 10% tolerance
            if (message?.Payload?.Session.KeepAliveTimeoutSeconds != null)
            {
                _keepAlive = (message.Payload.Session.KeepAliveTimeoutSeconds.Value * 1000 + 100);
            }
            _watchdog.Start(_keepAlive);
        }

        private async Task NotificationMessageProcessingAsync(WebSocketNotificationMessage message)
        {
            if (message.Payload != null)
                await OnNotificationMessageAsync.TryInvoke(this, message.Payload);
        }

        private async Task ReconnectMessageProcessingAsync(WebSocketReconnectMessage message)
        {


            _awaitForReconnect = true;
            _watchdog.Stop();
            await _socket.DisconnectAsync();
            if (message?.Payload?.Session.ReconnectUrl != null)
            {
                _connectionActive = await _socket.ConnectAsync(message.Payload.Session.ReconnectUrl);
                if (!_connectionActive)
                {
                    _logger.LogInformation("[EventSubClient] - [EventSubSocketWrapper] connection lost during reconnect");
                    return;
                }
            }
            _watchdog.Start(_keepAlive);
            SessionId = message?.Payload?.Session.Id;

            await OnRegisterSubscriptionsAsync.TryInvoke(this, SessionId);

            _awaitForReconnect = false;
        }

        private async Task RevocationMessageProcessingAsync(WebSocketRevocationMessage message)
        {
            if (OnRevocationMessageAsync != null)
            {
                await OnRevocationMessageAsync.TryInvoke(this, message);
            }
        }

        private Task PingMessageProcessingAsync()
        {
            if (_awaitForReconnect == false)
            {
                _socket.Send("Pong");
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }

        private Task KeepAliveMessageProcessing()
        {
            _watchdog.Reset();
            return Task.CompletedTask;
        }

        private async Task OnWatchdogTimeoutAsync(object sender, string e)
        {
            await _socket.DisconnectAsync();
            _logger.LogInformation("[EventSubClient] - [EventSubSocketWrapper] Server didn't respond in time");
            if (OnOutsideDisconnectAsync != null)
            {
                await OnOutsideDisconnectAsync.TryInvoke(this, e);
            }
            _connectionActive = false;
        }
    }

}
