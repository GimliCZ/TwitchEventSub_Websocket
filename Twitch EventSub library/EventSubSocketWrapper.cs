using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Twitch_EventSub_library.CoreFunctions;
using Twitch_EventSub_library.Messages;
using Twitch_EventSub_library.Messages.KeepAliveMessage;
using Twitch_EventSub_library.Messages.NotificationMessage;
using Twitch_EventSub_library.Messages.NotificationMessage.Events;
using Twitch_EventSub_library.Messages.PingMessage;
using Twitch_EventSub_library.Messages.ReconnectMessage;
using Twitch_EventSub_library.Messages.RevocationMessage;
using Twitch_EventSub_library.Messages.SharedContents;
using Twitch_EventSub_library.Messages.WelcomeMessage;

namespace Twitch_EventSub_library
{
    public class EventSubSocketWrapper
    {
        private const string DefaultWebSocketUrl = "wss://eventsub.wss.twitch.tv/ws";
        private readonly ILogger<EventSubSocketWrapper> _logger;
        private readonly ReplayProtection _replayProtection;
        private bool _awaitForReconnect = false;
        private readonly Watchdog _watchdog;
#if DEBUG
        public string _sessionId;
        public GenericWebsocket _socket;
#else
        private string _sessionId;
        private readonly GenericWebsocket _socket;
#endif

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
            return ParseWebSocketMessageAsync(e);
        }

        public Task ParseWebSocketMessageAsync(string e)
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
                    _logger.LogError("Error while parsing WebSocket message: " + ex.Message, ex);
                    return Task.CompletedTask;
                }
#if !DEBUG
                

                if (message == null ||
                    _replayProtection.IsDuplicate(message.Metadata.MessageId) ||
                    !_replayProtection.IsUpToDate(message.Metadata.MessageTimestamp))
                {
                    return Task.CompletedTask;
                }
#endif
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
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions and log them
                _logger.LogError("Unexpected error while processing WebSocket message: " + ex.Message, ex);
                return Task.CompletedTask;
            }
        }

        private WebSocketMessage DeserializeMessage(string message)
        {
            JObject jsonObject = JObject.Parse(message);

            if (!jsonObject.TryGetValue("metadata", out JToken? metadataToken) || !(metadataToken is JObject metadataObject))
            {
                throw new JsonSerializationException("metadata is missing in the JSON object");
            }
            var metadata = metadataToken.ToObject<WebSocketMessageMetadata>();

            string messageType = metadata?.MessageType;

            if (!jsonObject.TryGetValue("payload", out JToken? payloadToken) || !(payloadToken is JObject payloadObject))
            {
                throw new JsonSerializationException("metadata is missing in the JSON object");
            }



            return messageType switch
            {
                "session_welcome" => new WebSocketWelcomeMessage()
                {
                    Metadata = metadata,
                    Payload = payloadToken?.ToObject<WebSocketWelcomePayload>()
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

        private WebSocketNotificationPayload CreateNotificationPayload(JToken payload)
        {
            var resultMessage = new WebSocketNotificationPayload();

            resultMessage.Subscription = payload["subscription"]?.ToObject<WebSocketSubscription>();

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
                    break; //WebSocketNotificationEvent

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

                /*  case "channel.channel_points_custom_reward.add":
                      resultMessage.Event = payload["event"]?.ToObject<channe>();
                      break;

                  case "channel.channel_points_custom_reward.update":
                      if (message.Payload.Event is ChannelPointsCustomRewardUpdateEvent pointsCustomRewardUpdateEvent)
                      {
                          // Handle ChannelPointsCustomRewardUpdateEvent here...
                      }
                      break;

                  case "channel.channel_points_custom_reward.remove":
                      if (message.Payload.Event is ChannelPointsCustomRewardRemoveEvent pointsCustomRewardRemoveEvent)
                      {
                          // Handle ChannelPointsCustomRewardRemoveEvent here...
                      }
                      break;

                  case "channel.channel_points_custom_reward_redemption.add":
                      if (message.Payload.Event is ChannelPointsCustomRewardRedemptionAddEvent pointsCustomRewardRedemptionAddEvent)
                      {
                          // Handle ChannelPointsCustomRewardRedemptionAddEvent here...
                      }
                      break;

                  case "channel.channel_points_custom_reward_redemption.update":
                      if (message.Payload.Event is ChannelPointsCustomRewardRedemptionUpdateEvent pointsCustomRewardRedemptionUpdateEvent)
                      {
                          // Handle ChannelPointsCustomRewardRedemptionUpdateEvent here...
                      }
                      break;

                  case "channel.poll.begin":
                      if (message.Payload.Event is ChannelPollBeginEvent pollBeginEvent)
                      {
                          // Handle ChannelPollBeginEvent here...
                      }
                      break;

                  case "channel.poll.progress":
                      if (message.Payload.Event is ChannelPollProgressEvent pollProgressEvent)
                      {
                          // Handle ChannelPollProgressEvent here...
                      }
                      break;

                  case "channel.poll.end":
                      if (message.Payload.Event is ChannelPollEndEvent pollEndEvent)
                      {
                          // Handle ChannelPollEndEvent here...
                      }
                      break;

                  case "channel.prediction.begin":
                      if (message.Payload.Event is ChannelPredictionBeginEvent predictionBeginEvent)
                      {
                          // Handle ChannelPredictionBeginEvent here...
                      }
                      break;

                  case "channel.prediction.progress":
                      if (message.Payload.Event is ChannelPredictionProgressEvent predictionProgressEvent)
                      {
                          // Handle ChannelPredictionProgressEvent here...
                      }
                      break;

                  case "channel.prediction.lock":
                      if (message.Payload.Event is ChannelPredictionLockEvent predictionLockEvent)
                      {
                          // Handle ChannelPredictionLockEvent here...
                      }
                      break;

                  case "channel.prediction.end":
                      if (message.Payload.Event is ChannelPredictionEndEvent predictionEndEvent)
                      {
                          // Handle ChannelPredictionEndEvent here...
                      }
                      break;

                  case "channel.charity_campaign.donate":
                      if (message.Payload.Event is ChannelCharityCampaignDonateEvent charityCampaignDonateEvent)
                      {
                          // Handle ChannelCharityCampaignDonateEvent here...
                      }
                      break;

                  case "channel.charity_campaign.start":
                      if (message.Payload.Event is ChannelCharityCampaignStartEvent charityCampaignStartEvent)
                      {
                          // Handle ChannelCharityCampaignStartEvent here...
                      }
                      break;

                  case "channel.charity_campaign.progress":
                      if (message.Payload.Event is ChannelCharityCampaignProgressEvent charityCampaignProgressEvent)
                      {
                          // Handle ChannelCharityCampaignProgressEvent here...
                      }
                      break;

                  case "channel.charity_campaign.stop":
                      if (message.Payload.Event is ChannelCharityCampaignStopEvent charityCampaignStopEvent)
                      {
                          // Handle ChannelCharityCampaignStopEvent here...
                      }
                      break;

                  case "drop.entitlement.grant":
                      if (message.Payload.Event is DropEntitlementGrantEvent entitlementGrantEvent)
                      {
                          // Handle DropEntitlementGrantEvent here...
                      }
                      break;

                  case "extension.bits_transaction.create":
                      if (message.Payload.Event is ExtensionBitsTransactionCreateEvent bitsTransactionCreateEvent)
                      {
                      }*/
                default:
                    break;
            }
            return resultMessage;
        }

        private async Task WelcomeMessageProcessing(WebSocketWelcomeMessage message)
        {
            _sessionId = message.Payload.Session.Id;
            if (OnRegisterSubscriptions != null)
            {
                await OnRegisterSubscriptions.Invoke(this, _sessionId);
            }
            // keep alive in sec + 10% tolerance
            if (message.Payload.Session.KeepAliveTimeoutSeconds != null)
            {
                _keepAlive = (message.Payload.Session.KeepAliveTimeoutSeconds.Value * 1000 + 100);
            }
            _watchdog.Start(_keepAlive);
        }

        private async Task NotificationMessageProcessing(WebSocketNotificationMessage message)
        {
            if (OnNotificationMessage != null)
            {
                await OnNotificationMessage.Invoke(this, message.Payload);
            }
        }

        private async Task ReconnectMessageProcessing(WebSocketReconnectMessage message)
        {
#if !DEBUG
            

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
            if (OnRegisterSubscriptions != null)
            {
                await OnRegisterSubscriptions.Invoke(this, _sessionId);
            }
            _awaitForReconnect = false;
#else
            _sessionId = message.Payload.Session.Id;
            await OnRegisterSubscriptions.Invoke(this, _sessionId);
#endif
        }

        private async Task RevocationMessageProcessing(WebSocketRevocationMessage message)
        {
            if (OnRevocationMessage != null)
            {
                await OnRevocationMessage.Invoke(this, message);
            }
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
            if (OnOutsideDisconnect != null)
            {
                await OnOutsideDisconnect.Invoke(this, e);
            }
            _connectionActive = false;
        }
    }

}
