namespace Twitch_EventSub_library.CoreFunctions
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
            var now = DateTimeOffset.UtcNow;
            var messageTime = ConvertToRfc3339WithNanoseconds(data);
            return (now - messageTime) < TimeSpan.FromMinutes(10);

        }

        public static DateTimeOffset ConvertToRfc3339WithNanoseconds(string timestamp)
        {
            DateTime dateTime = DateTime.Parse(timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind);

            // Get the timestamp in UTC to correctly represent the offset.
            return new DateTimeOffset(dateTime.ToUniversalTime());
        }
    }
}
