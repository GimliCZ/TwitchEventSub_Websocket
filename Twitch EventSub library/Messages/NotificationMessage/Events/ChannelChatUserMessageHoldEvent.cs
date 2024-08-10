namespace Twitch.EventSub.Messages.NotificationMessage.Events
{
    public class ChannelChatUserMessageHoldEvent : WebSocketNotificationEvent
    {
        public string user_id { get; set; }
        public string user_login { get; set; }
        public string user_name { get; set; }
        public string message_id { get; set; }
        public Message message { get; set; }
    }
}