using Microsoft.Extensions.Logging;


namespace Twitch.EventSub.CoreFunctions
{
    public static class LoggerExtension
    {
        public static ILogger LogDetails(this ILogger logger, LogLevel logLevel, params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (!logger.IsEnabled(logLevel))
                return logger;

            var message = string.Join(Environment.NewLine, args.OfType<string>());
            var details = string.Join(Environment.NewLine, args.Except(args.OfType<string>()).Select(d => d is Exception ? FormatExceptionDetails(d as Exception) : Newtonsoft.Json.JsonConvert.SerializeObject(d)));

            var formattedMessage = $"{message}{Environment.NewLine}Details:{Environment.NewLine}{details}";

            switch (logLevel)
            {
                case LogLevel.Trace:
                    logger.LogTrace(formattedMessage);
                    break;
                case LogLevel.Debug:
                    logger.LogDebug(formattedMessage);
                    break;
                case LogLevel.Information:
                    logger.LogInformation(formattedMessage);
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    logger.LogError(formattedMessage);
                    break;
                case LogLevel.Critical:
                    logger.LogCritical(formattedMessage);
                    break;
                default:
                    logger.LogInformation(formattedMessage);
                    break;
            }

            return logger;
        }

        public static ILogger LogTraceDetails(this ILogger logger, params object[] args) => LogDetails(logger, LogLevel.Trace, args);

        public static ILogger LogDebugDetails(this ILogger logger, params object[] args) => LogDetails(logger, LogLevel.Debug, args);

        public static ILogger LogInformationDetails(this ILogger logger, params object[] args) => LogDetails(logger, LogLevel.Information, args);

        public static ILogger LogWarningDetails(this ILogger logger, params object[] args) => LogDetails(logger, LogLevel.Warning, args);

        public static ILogger LogErrorDetails(this ILogger logger, params object[] args) => LogDetails(logger, LogLevel.Error, args);

        public static ILogger LogCriticalDetails(this ILogger logger, params object[] args) => LogDetails(logger, LogLevel.Critical, args);

        private static string FormatExceptionDetails(Exception? exception)
        {
            if (exception == null)
                return string.Empty;
            var details = $"Exception Type: {exception.GetType().FullName}{Environment.NewLine}";
            details += $"Message: {exception.Message}{Environment.NewLine}";
            details += $"Source: {exception.Source}{Environment.NewLine}";
            details += $"Stack Trace:{Environment.NewLine}{exception.StackTrace}{Environment.NewLine}";


            if (exception.InnerException != null)
                details += $"Inner Exception: {FormatExceptionDetails(exception.InnerException)}{Environment.NewLine}";

            return details;
        }
    }
}