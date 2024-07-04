namespace Twitch.EventSub.API.Models
{
    public class InvalidAccessTokenException : Exception
    {
        public string SourceUserId = string.Empty;
        public DateTime Date;
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
