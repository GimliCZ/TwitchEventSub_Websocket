﻿using Microsoft.Extensions.Logging;

namespace Twitch.EventSub.CoreFunctions
{
    public class Watchdog
    {
        private Timer _timerWatchdog;
        private int _timeout;
        private bool _isRunning;
        private readonly ILogger _logger;

        public event AsyncEventHandler<string> OnWatchdogTimeout;

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
                _timerWatchdog = new Timer(OnTimerElapsed, null, _timeout, _timeout);
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
            if (_timerWatchdog == null)
            {
                throw new ArgumentNullException(nameof(_timerWatchdog));
            }
            if (_isRunning)
            {
                _timerWatchdog.Change(_timeout, Timeout.Infinite);
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
            if (_timerWatchdog == null)
            {
                throw new ArgumentNullException(nameof(_timerWatchdog));
            }
            if (_isRunning)
            {
                _timerWatchdog.Change(Timeout.Infinite, Timeout.Infinite);
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
            _timerWatchdog.Change(Timeout.Infinite, Timeout.Infinite);
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
