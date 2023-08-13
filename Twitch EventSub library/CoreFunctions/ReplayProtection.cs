namespace Twitch.EventSub.CoreFunctions
{
    /// <summary>
    /// This class is derived from Twitch documentation.
    /// </summary>
    public class ReplayProtection
    {
        private readonly Queue<string>? _rememberedMessages; //better performance for small sizes
        private readonly int _memoryMaxSize;
        public ReplayProtection(int messagesToRemember)
        {
            _memoryMaxSize = messagesToRemember;
            _rememberedMessages = new Queue<string>();
        }
        public bool IsDuplicate(string data)
        {
            if (_rememberedMessages?.Contains(data) == true)
            {

                return true;
            }

            if (_rememberedMessages?.Count >= _memoryMaxSize)
            {
                _rememberedMessages.Dequeue();
            }
            _rememberedMessages?.Enqueue(data);
            return false;
        }

        public bool IsUpToDate(string data)
        {
            var now = DateTime.UtcNow;
            var messageTime = ConvertToRfc3339WithNanoseconds(data);
            return (now - messageTime) < TimeSpan.FromMinutes(10);

        }

        public static DateTime ConvertToRfc3339WithNanoseconds(string timestamp)
        {
            return DateTime.Parse(timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime();
        }
    }
}
