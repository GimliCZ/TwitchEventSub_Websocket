namespace Twitch.EventSub
{
    public class EventSubClientOptions
    {
        public TimeSpan CommunicationSpeed { get; }

        public EventSubClientOptions(TimeSpan? communicationSpeed = null)
        {
            CommunicationSpeed = communicationSpeed ?? TimeSpan.FromMilliseconds(300);
        }
    }
}
