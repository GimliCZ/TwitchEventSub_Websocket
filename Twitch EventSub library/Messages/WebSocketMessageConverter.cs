using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Twitch_EventSub_library.Messages.KeepAliveMessage;
using Twitch_EventSub_library.Messages.NotificationMessage;
using Twitch_EventSub_library.Messages.PingMessage;
using Twitch_EventSub_library.Messages.ReconnectMessage;
using Twitch_EventSub_library.Messages.RevocationMessage;
using Twitch_EventSub_library.Messages.WelcomeMessage;

namespace Twitch_EventSub_library.Messages
{
    public class WebSocketMessageConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(WebSocketMessage);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            //We can use message type as key to identify class

            if (!jsonObject.TryGetValue("message_type", StringComparison.OrdinalIgnoreCase, out var messageTypeToken))
                throw new JsonSerializationException("message_type is missing in the JSON object");
            var messageType = messageTypeToken.ToString();
            return messageType switch
            {
                "session_welcome" => jsonObject.ToObject<WebSocketWelcomeMessage>(serializer),
                "session_keepalive" => jsonObject.ToObject<WebSocketKeepAliveMessage>(serializer),
                "ping" => jsonObject.ToObject<WebSocketPingMessage>(serializer),
                "notification" => jsonObject.ToObject<WebSocketNotificationMessage>(serializer),
                "session_reconnect" => jsonObject.ToObject<WebSocketReconnectMessage>(serializer),
                "revocation" => jsonObject.ToObject<WebSocketRevocationMessage>(serializer),
                _ => throw new JsonSerializationException($"Unsupported message_type: {messageType}")
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //we will never write this json. Just read
            throw new NotImplementedException();
        }
    }
}
