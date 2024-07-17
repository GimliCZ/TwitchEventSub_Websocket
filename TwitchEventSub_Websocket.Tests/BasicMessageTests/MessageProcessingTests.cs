using Twitch.EventSub.Messages.KeepAliveMessage;
using Twitch.EventSub.Messages.NotificationMessage;
using Twitch.EventSub.Messages.PingMessage;
using Twitch.EventSub.Messages.ReconnectMessage;
using Twitch.EventSub.Messages.RevocationMessage;
using Twitch.EventSub.Messages.WelcomeMessage;
using Twitch.EventSub.User;

namespace TwitchEventSub_Websocket.Tests.BasicMessageTests
{
    public class MessageProcessingTests
    {
        [Fact]
        public async Task DeserializeMessageAsync_WhenGivenSessionWelcomeMessage_ReturnsWebSocketWelcomeMessage()
        {
            // Arrange
            const string? message = @"{
                ""metadata"": {
                    ""message_id"": ""96a3f3b5-5dec-4eed-908e-e11ee657416c"",
                    ""message_type"": ""session_welcome"",
                    ""message_timestamp"": ""2023-07-19T14:56:51.634234626Z""
                },
                ""payload"": {
                    ""session"": {
                        ""id"": ""AQoQILE98gtqShGmLD7AM6yJThAB"",
                        ""status"": ""connected"",
                        ""connected_at"": ""2023-07-19T14:56:51.616329898Z"",
                        ""keepalive_timeout_seconds"": 10,
                        ""reconnect_url"": null
                    }
                }
            }";

            // Act
            var result = await MessageProcessing.DeserializeMessageAsync(message);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<WebSocketWelcomeMessage>(result);
            var welcomeMessage = (WebSocketWelcomeMessage)result;
            Assert.Equal("session_welcome", welcomeMessage.Metadata.MessageType);
            Assert.Equal("96a3f3b5-5dec-4eed-908e-e11ee657416c", welcomeMessage.Metadata.MessageId);
            Assert.Equal("AQoQILE98gtqShGmLD7AM6yJThAB", welcomeMessage?.Payload?.Session?.Id);
        }

        [Fact]
        public async Task DeserializeMessageAsync_WhenGivenSessionKeepaliveMessage_ReturnsWebSocketKeepaliveMessage()
        {
            // Arrange
            const string? message = @"{
                ""metadata"": {
                    ""message_id"": ""84c1e79a-2a4b-4c13-ba0b-4312293e9308"",
                    ""message_type"": ""session_keepalive"",
                    ""message_timestamp"": ""2023-07-19T10:11:12.634234626Z""
                },
                ""payload"": {}
            }";

            // Act
            var result = await MessageProcessing.DeserializeMessageAsync(message);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<WebSocketKeepAliveMessage>(result);
            var keepaliveMessage = (WebSocketKeepAliveMessage)result;
            Assert.Equal("session_keepalive", keepaliveMessage.Metadata.MessageType);
            Assert.Equal("84c1e79a-2a4b-4c13-ba0b-4312293e9308", keepaliveMessage.Metadata.MessageId);
        }

        [Fact]
        public async Task DeserializeMessageAsync_WhenGivenNotificationMessage_ReturnsWebSocketNotificationMessage()
        {
            // Arrange
            const string? message = @"{
                ""metadata"": {
                    ""message_id"": ""befa7b53-d79d-478f-86b9-120f112b044e"",
                    ""message_type"": ""notification"",
                    ""message_timestamp"": ""2022-11-16T10:11:12.464757833Z"",
                    ""subscription_type"": ""channel.follow"",
                    ""subscription_version"": ""1""
                },
                ""payload"": {
                    ""subscription"": {
                        ""id"": ""f1c2a387-161a-49f9-a165-0f21d7a4e1c4"",
                        ""status"": ""enabled"",
                        ""type"": ""channel.follow"",
                        ""version"": ""1"",
                        ""cost"": 1,
                        ""condition"": {
                            ""broadcaster_user_id"": ""12826""
                        },
                        ""transport"": {
                            ""method"": ""websocket"",
                            ""session_id"": ""AQoQexAWVYKSTIu4ec_2VAxyuhAB""
                        },
                        ""created_at"": ""2022-11-16T10:11:12.464757833Z""
                    },
                    ""event"": {
                        ""user_id"": ""1337"",
                        ""user_login"": ""awesome_user"",
                        ""user_name"": ""Awesome_User"",
                        ""broadcaster_user_id"": ""12826"",
                        ""broadcaster_user_login"": ""twitch"",
                        ""broadcaster_user_name"": ""Twitch"",
                        ""followed_at"": ""2023-07-15T18:16:11.17106713Z""
                    }
                }
            }";

            // Act
            var result = await MessageProcessing.DeserializeMessageAsync(message);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<WebSocketNotificationMessage>(result);
            var notificationMessage = (WebSocketNotificationMessage)result;
            Assert.Equal("notification", notificationMessage.Metadata.MessageType);
            Assert.Equal("befa7b53-d79d-478f-86b9-120f112b044e", notificationMessage.Metadata.MessageId);
            Assert.Equal("channel.follow", notificationMessage?.Payload?.Subscription?.Type);
        }

        [Fact]
        public async Task DeserializeMessageAsync_WhenGivenPingMessage_ReturnsWebSocketPingMessage()
        {
            // Arrange
            const string? message = @"{
                ""metadata"": {
                    ""message_id"": ""84c1e79a-2a4b-4c13-ba0b-4312293e9308"",
                    ""message_type"": ""ping"",
                    ""message_timestamp"": ""2023-07-19T10:11:12.634234626Z""
                },
                ""payload"": {}
            }";

            // Act
            var result = await MessageProcessing.DeserializeMessageAsync(message);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<WebSocketPingMessage>(result);
            var pingMessage = (WebSocketPingMessage)result;
            Assert.Equal("ping", pingMessage.Metadata.MessageType);
            Assert.Equal("84c1e79a-2a4b-4c13-ba0b-4312293e9308", pingMessage.Metadata.MessageId);
        }

        [Fact]
        public async Task DeserializeMessageAsync_WhenGivenSessionReconnectMessage_ReturnsWebSocketReconnectMessage()
        {
            // Arrange
            const string? message = @"{
                ""metadata"": {
                    ""message_id"": ""84c1e79a-2a4b-4c13-ba0b-4312293e9308"",
                    ""message_type"": ""session_reconnect"",
                    ""message_timestamp"": ""2022-11-18T09:10:11.634234626Z""
                },
                ""payload"": {
                    ""session"": {
                       ""id"": ""AQoQexAWVYKSTIu4ec_2VAxyuhAB"",
                       ""status"": ""reconnecting"",
                       ""keepalive_timeout_seconds"": null,
                       ""reconnect_url"": ""wss://eventsub.wss.twitch.tv?..."",
                       ""connected_at"": ""2022-11-16T10:11:12.634234626Z""
                    }
                }
            }";

            // Act
            var result = await MessageProcessing.DeserializeMessageAsync(message);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<WebSocketReconnectMessage>(result);
            var reconnectMessage = (WebSocketReconnectMessage)result;
            Assert.Equal("session_reconnect", reconnectMessage.Metadata.MessageType);
            Assert.Equal("84c1e79a-2a4b-4c13-ba0b-4312293e9308", reconnectMessage.Metadata.MessageId);
            Assert.Equal("wss://eventsub.wss.twitch.tv?...", reconnectMessage?.Payload?.Session?.ReconnectUrl);
        }

        [Fact]
        public async Task DeserializeMessageAsync_WhenGivenRevocationMessage_ReturnsWebSocketRevocationMessage()
        {
            // Arrange
            const string? message = @"{
                ""metadata"": {
                    ""message_id"": ""84c1e79a-2a4b-4c13-ba0b-4312293e9308"",
                    ""message_type"": ""revocation"",
                    ""message_timestamp"": ""2022-11-16T10:11:12.464757833Z"",
                    ""subscription_type"": ""channel.follow"",
                    ""subscription_version"": ""1""
                },
                ""payload"": {
                    ""subscription"": {
                        ""id"": ""f1c2a387-161a-49f9-a165-0f21d7a4e1c4"",
                        ""status"": ""authorization_revoked"",
                        ""type"": ""channel.follow"",
                        ""version"": ""1"",
                        ""cost"": 1,
                        ""condition"": {
                            ""broadcaster_user_id"": ""12826""
                        },
                        ""transport"": {
                            ""method"": ""websocket"",
                            ""session_id"": ""AQoQexAWVYKSTIu4ec_2VAxyuhAB""
                        },
                        ""created_at"": ""2022-11-16T10:11:12.464757833Z""
                    }
                }
            }";

            // Act
            var result = await MessageProcessing.DeserializeMessageAsync(message);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<WebSocketRevocationMessage>(result);
            var revocationMessage = (WebSocketRevocationMessage)result;
            Assert.Equal("revocation", revocationMessage.Metadata.MessageType);
            Assert.Equal("84c1e79a-2a4b-4c13-ba0b-4312293e9308", revocationMessage.Metadata.MessageId);
            Assert.Equal("authorization_revoked", revocationMessage?.Payload?.Subscription?.Status);
        }
    }
}
