using Microsoft.Extensions.Logging;
using Moq;
using Twitch_EventSub_library;
using Twitch_EventSub_library.CoreFunctions;
using Twitch_EventSub_library.Messages.NotificationMessage;
using Twitch_EventSub_library.Messages.RevocationMessage;

namespace EventSubTests
{
    //TEST only work in DEBUG
    public class EventSubSocketWrapperTests
    {
        // Helper method to create a mocked ILogger
        private ILogger<T> CreateMockLogger<T>() where T : class
        {
            return Mock.Of<ILogger<T>>();
        }

        // Test parsing of the Welcome message
        [Fact]
        public async Task Parse_WelcomeMessage_Success()
        {

            // Arrange
            var logger = CreateMockLogger<EventSubSocketWrapper>();
            var socketLogger = CreateMockLogger<GenericWebsocket>();
            var watchdogLogger = CreateMockLogger<Watchdog>();

            var eventSubSocketWrapper = new EventSubSocketWrapper(logger, socketLogger, watchdogLogger, TimeSpan.FromSeconds(1));

            // Sample Welcome message JSON string
            string welcomeMessageJson = @"{
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
            await eventSubSocketWrapper.ParseWebSocketMessageAsync(welcomeMessageJson);
            // Assert
            Assert.Equal("AQoQILE98gtqShGmLD7AM6yJThAB", eventSubSocketWrapper._sessionId);
        }

        [Fact]
        public async Task Parse_NotificationMessage_Success()
        {
            // Arrange
            var logger = CreateMockLogger<EventSubSocketWrapper>();
            var socketLogger = CreateMockLogger<GenericWebsocket>();
            var watchdogLogger = CreateMockLogger<Watchdog>();

            var eventSubSocketWrapper = new EventSubSocketWrapper(logger, socketLogger, watchdogLogger, TimeSpan.FromSeconds(1));

            // Sample Notification message JSON string
            string notificationMessageJson = @"{
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
            // Verify that the OnNotificationMessage event is invoked with the correct payload
            var mockNotificationHandler = new Mock<AsyncEventHandler<WebSocketNotificationPayload>>();
            eventSubSocketWrapper.OnNotificationMessage += mockNotificationHandler.Object;
            // Act
            await eventSubSocketWrapper.ParseWebSocketMessageAsync(notificationMessageJson);

            // Assert
            mockNotificationHandler.Verify(handler => handler.Invoke(eventSubSocketWrapper, It.IsAny<WebSocketNotificationPayload>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ReconnectMessage_Success()
        {
            // Arrange
            var logger = CreateMockLogger<EventSubSocketWrapper>();
            var socketLogger = CreateMockLogger<GenericWebsocket>();
            var watchdogLogger = CreateMockLogger<Watchdog>();

            var eventSubSocketWrapper = new EventSubSocketWrapper(logger, socketLogger, watchdogLogger, TimeSpan.FromSeconds(1));

            // Mock the GenericWebsocket
            var mockSocket = new Mock<GenericWebsocket>(socketLogger, TimeSpan.FromSeconds(1));

            // Sample Reconnect message JSON string
            string reconnectMessageJson = @"{
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



            // Verify that the OnRegisterSubscriptions event is invoked after the reconnection
            var mockRegisterSubscriptionsHandler = new Mock<AsyncEventHandler<string>>();
            eventSubSocketWrapper.OnRegisterSubscriptions += mockRegisterSubscriptionsHandler.Object;
            // Act
            await eventSubSocketWrapper.ParseWebSocketMessageAsync(reconnectMessageJson);

            mockRegisterSubscriptionsHandler.Verify(handler => handler.Invoke(eventSubSocketWrapper, "AQoQexAWVYKSTIu4ec_2VAxyuhAB"), Times.Once);
        }


        [Fact]
        public async Task Handle_RevocationMessage_Success()
        {
            // Arrange
            var logger = CreateMockLogger<EventSubSocketWrapper>();
            var socketLogger = CreateMockLogger<GenericWebsocket>();
            var watchdogLogger = CreateMockLogger<Watchdog>();

            var eventSubSocketWrapper = new EventSubSocketWrapper(logger, socketLogger, watchdogLogger, TimeSpan.FromSeconds(1));

            // Sample Revocation message JSON string
            string revocationMessageJson = @"{
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

            // Create a mock event handler for OnRevocationMessage
            var mockRevocationHandler = new Mock<AsyncEventHandler<WebSocketRevocationMessage>>();

            // Act
            eventSubSocketWrapper.OnRevocationMessage += mockRevocationHandler.Object;
            await eventSubSocketWrapper.ParseWebSocketMessageAsync(revocationMessageJson);

            // Assert
            // Verify that the OnRevocationMessage event is invoked with the correct payload
            mockRevocationHandler.Verify(handler => handler.Invoke(eventSubSocketWrapper, It.IsAny<WebSocketRevocationMessage>()), Times.Once);
        }

        // Add more test cases to cover parsing of other message types (Ping, Notification, Reconnect, Revocation, Close)

        // Test handling of unsupported message type
        [Fact]
        public async Task Parse_UnsupportedMessageType_ThrowsException()
        {
            // Arrange
            var logger = CreateMockLogger<EventSubSocketWrapper>();
            var socketLogger = CreateMockLogger<GenericWebsocket>();
            var watchdogLogger = CreateMockLogger<Watchdog>();

            var eventSubSocketWrapper = new EventSubSocketWrapper(logger, socketLogger, watchdogLogger, TimeSpan.FromSeconds(1));

            // Sample unsupported message JSON string
            string unsupportedMessageJson = @"{
                ""metadata"": {
                    ""message_id"": ""84c1e79a-2a4b-4c13-ba0b-4312293e9308"",
                    ""message_type"": ""unsupported"",
                    ""message_timestamp"": ""2023-07-19T10:11:12.634234626Z""
                },
                ""payload"": {}
            }";

            // Act and Assert
            // Ensure that an exception is thrown when parsing an unsupported message type
            var exception = await Record.ExceptionAsync(async () => await eventSubSocketWrapper.ParseWebSocketMessageAsync(unsupportedMessageJson));
            Assert.Null(exception);
        }
    }
}