namespace Twitch.EventSub.API.Models
{
    public class InvalidAccessTokenException : Exception
    {
        public DateTime Date { get; }
        public string SourceUserId { get; init; }

        public InvalidAccessTokenException()
        {
            Date = DateTime.Now;
        }

        public InvalidAccessTokenException(string message) : base(message)
        {
            Date = DateTime.Now;
        }

        public InvalidAccessTokenException(string message, Exception innerException) : base(message, innerException)
        {
            Date = DateTime.Now;
        }
    }
}