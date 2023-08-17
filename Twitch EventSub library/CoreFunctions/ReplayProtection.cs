using System.Globalization;

namespace Twitch.EventSub.CoreFunctions
{
    /// <summary>
    /// This class is derived from Twitch documentation.
    /// </summary>
    public class ReplayProtection
    {
        private static readonly string format = "MM/dd/yyyy HH:mm:ss";
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
            var messageTime = ParseDateTimeString(data);
            return (now - messageTime) < TimeSpan.FromMinutes(10);

        }

        public static DateTime ParseDateTimeString(string timestamp)
        {
            //ConvertToRfc3339WithNanoseconds
            if (DateTime.TryParse(timestamp,null, System.Globalization.DateTimeStyles.RoundtripKind, out var dateTime))
            {
                return dateTime.ToUniversalTime();
            }
            //alternative
            //example: string dateString = "08/17/2023 20:33:14";
            if (DateTime.TryParseExact(timestamp, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result.ToUniversalTime();
            }
            throw new Exception("Parsed Invalid date");
        }
    }
}
