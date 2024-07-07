using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Twitch.EventSub.Messages;
using Twitch.EventSub.Messages.KeepAliveMessage;
using Twitch.EventSub.Messages.NotificationMessage;
using Twitch.EventSub.Messages.NotificationMessage.Events;
using Twitch.EventSub.Messages.PingMessage;
using Twitch.EventSub.Messages.ReconnectMessage;
using Twitch.EventSub.Messages.RevocationMessage;
using Twitch.EventSub.Messages.SharedContents;
using Twitch.EventSub.Messages.WelcomeMessage;

namespace Twitch.EventSub.User
{
    public static class MessageProcessing
    {
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

                case "channel.chat.clear":
                    resultMessage.Event = payload["event"]?.ToObject<ChannelChatClearEvent>();
                    break;
                case "channel.chat.clear_user_messages":
                    resultMessage.Event = payload["event"]?.ToObject<ChannelUserChatClearEvent>();
                    break;

                case "channel.chat.message":
                    resultMessage.Event = payload["event"]?.ToObject<ChannelChatMessage>();
                    break;

                case "channel.chat.message_delete":
                    resultMessage.Event = payload["event"]?.ToObject<ChannelChatMessageDeleteEvent>();
                    break;

                case "channel.chat.notification":
                    resultMessage.Event = payload["event"]?.ToObject<ChannelChatNotificationEvent>();
                    break;

                case "channel.chat_settings.update":
                    resultMessage.Event = payload["event"]?.ToObject<ChannelChatSettingsUpdateEvent>();
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

        public static async Task<WebSocketMessage> DeserializeMessageAsync(string message)
        {
            using (JsonTextReader reader = new JsonTextReader(new StringReader(message)))
            {
                reader.SupportMultipleContent = true;
                while (await reader.ReadAsync())
                {
                    JObject jsonObject = JObject.Load(reader);
                    if (!jsonObject.TryGetValue("metadata", out JToken? metadataToken) || !(metadataToken is JObject))
                    {
                        throw new JsonSerializationException($"metadata is missing in the JSON object {message}");
                    }
                    var metadata = metadataToken.ToObject<WebSocketMessageMetadata>();
                    if (metadata == null)
                    {
                        throw new JsonSerializationException();
                    }
                    string messageType = metadata.MessageType;

                    if (!jsonObject.TryGetValue("payload", out JToken? payloadToken) || !(payloadToken is JObject))
                    {
                        throw new JsonSerializationException($"metadata is missing in the JSON object {message}");
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
                throw new JsonSerializationException($"JSON object was not correctly processed {message}");
            }
        }
    }
}
