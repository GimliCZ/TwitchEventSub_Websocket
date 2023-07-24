using Microsoft.Extensions.Logging;

namespace Twitch_EventSub_library.CoreFunctions
{
    public class Watchdog
    {
        private Timer _timer;
        private int _timeout;
        private bool _isRunning;
        private readonly ILogger _logger;

        public event AsyncEventHandler<string> WatchdogTimeout;

        public Watchdog(ILogger logger)
        {
            _isRunning = false;
            _logger = logger;
        }

        public void Start(int timeout)
        {
            if (timeout <= 0)
                throw new ArgumentException("Timeout should be greater than 0 milliseconds.");
            _timeout = timeout;

            if (!_isRunning)
            {
                _isRunning = true;
                _timer = new Timer(OnTimerElapsed, null, _timeout, Timeout.Infinite);
                _logger.LogDebug("Watchdog started.");
            }
            else
            {
                _logger.LogDebug("Watchdog is already running.");
            }
        }

        public void Reset()
        {
            if (_timer == null) throw new NullReferenceException();
            if (_isRunning)
            {
                _timer.Change(_timeout, Timeout.Infinite);
                _logger.LogDebug("Watchdog reset.");
            }
            else
            {
                _logger.LogDebug("Watchdog is not running. Please start it first.");
            }
        }

        public void Stop()
        {
            if (_timer == null) throw new NullReferenceException();
            if (_isRunning)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _isRunning = false;
                _logger.LogDebug("Watchdog stopped.");
            }
            else
            {
                _logger.LogDebug("Watchdog is not running.");
            }
        }

        private async void OnTimerElapsed(object? state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _isRunning = false;
            _logger.LogDebug("Watchdog timeout! Something went wrong.");

            // Raise the WatchdogTimeout event
            await WatchdogTimeout?.Invoke(this,"Server didn't respond in time")!;
        }
    }
}
