﻿namespace Twitch_EventSub_library.API.Models
{
    public class InvalidAccessTokenException : Exception
    {
        public InvalidAccessTokenException()
        {
        }

        public InvalidAccessTokenException(string message) : base(message)
        {
        }

        public InvalidAccessTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
