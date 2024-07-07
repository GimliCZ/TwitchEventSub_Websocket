namespace Twitch.EventSub.API.Models
{
    public class RefreshRequestArgs
    {
        public string UserId { get; init; }
        public DateTime DateTime { get; init; }
    }
}