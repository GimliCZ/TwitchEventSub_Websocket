using Newtonsoft.Json;
using Twitch.EventSub.API.Models;

namespace TwitchEventSub_Websocket.Tests.TwitchApiTests
{
    public class ApiTests
    {
        public ApiTests()
        {
        }

        [Fact]
        public void TestRequestSerializationForSubscription()
        {
            // Arrange
            var request = new CreateSubscriptionRequest
            {
                Type = "user.update",
                Version = "1",
                Condition = new Condition { UserId = "1234" },
                Transport = new Transport { Method = "conduit", ConduitId = "bfcfc993-26b1-b876-44d9-afe75a379dac" }
            };

            var expectedRequestJson = @"{
            ""type"": ""user.update"",
            ""version"": ""1"",
            ""condition"": {
                ""user_id"": ""1234""
            },
            ""transport"": {
                ""method"": ""conduit"",
                ""conduit_id"": ""bfcfc993-26b1-b876-44d9-afe75a379dac""
            }
        }";

            expectedRequestJson = expectedRequestJson.Replace("\r", "").Replace("\n", "").Replace(" ", "");
            var serializedRequest = JsonConvert.SerializeObject(request);

            // Assert
            Assert.Equal(expectedRequestJson, serializedRequest);
        }
    }
}