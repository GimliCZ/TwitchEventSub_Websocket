using Microsoft.Extensions.Logging;
using Twitch.EventSub.Library.CoreFunctions;

namespace Twitch.EventSub.CoreFunctions
{
    public class Watchdog
    {
        private Timer _timer;
        private int _timeout;
        private bool _isRunning;
        private readonly ILogger _logger;

        public event AsyncEventHandler<string, Watchdog> OnWatchdogTimeout;

        public Watchdog(ILogger logger)
        {
            _isRunning = false;
            _logger = logger;
        }
        /// <summary>
        /// Starts Watchdog
        /// </summary>
        /// <param name="timeout"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Start(int timeout)
        {
            if (timeout <= 0)
                throw new ArgumentException("[EventSubClient] - [Watchdog] Timeout should be greater than 0 milliseconds.");
            _timeout = timeout;

            if (!_isRunning)
            {
                _isRunning = true;
                _timer = new Timer(OnTimerElapsed, null, _timeout, _timeout);
                _logger.LogDebug("[EventSubClient] - [Watchdog] Watchdog started.");
            }
            else
            {
                _logger.LogDebug("[EventSubClient] - [Watchdog] Watchdog is already running.");
            }
        }
        /// <summary>
        /// Renews timing 
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void Reset()
        {
            if (_timer == null)
            {
                throw new ArgumentNullException(nameof(_timer));
            }
            if (_isRunning)
            {
                _timer.Change(_timeout, Timeout.Infinite);
                _logger.LogDebug("[EventSubClient] - [Watchdog] Watchdog reset.");
            }
            else
            {
                _logger.LogDebug("[EventSubClient] - [Watchdog] Watchdog is not running. Please start it first.");
            }
        }
        /// <summary>
        /// Stops watchdog
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void Stop()
        {
            if (_timer == null)
            {
                throw new ArgumentNullException(nameof(_timer));
            }
            if (_isRunning)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _isRunning = false;
                _logger.LogDebug("[EventSubClient] - [Watchdog] Watchdog stopped.");
            }
            else
            {
                _logger.LogDebug("[EventSubClient] - [Watchdog] Watchdog is not running.");
            }
        }
        /// <summary>
        /// Triggers when watched time is overdue
        /// </summary>
        /// <param name="state"></param>
        private async void OnTimerElapsed(object? state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _isRunning = false;

            // Raise the WatchdogTimeout event
            try
            {
                await OnWatchdogTimeout.TryInvoke(this, "[EventSubClient] - [Watchdog] Server didn't respond in time")!;
            }
            catch (Exception ex)
            {
                //catch any exceptions, we dont want crash
                _logger.LogWarningDetails("Watchdog detected exception", ex);
            }
        }
    }
}
