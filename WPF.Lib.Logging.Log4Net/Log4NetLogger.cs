using Microsoft.Extensions.Logging;

namespace WPF.Lib.Logging.Log4Net;

internal sealed class Log4NetLogger(log4net.ILog logger) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => logger.Logger.IsEnabledFor(log4net.Core.Level.Trace),
            LogLevel.Debug => logger.IsDebugEnabled,
            LogLevel.Information => logger.IsInfoEnabled,
            LogLevel.Warning => logger.IsWarnEnabled,
            LogLevel.Error => logger.IsErrorEnabled,
            LogLevel.Critical => logger.IsFatalEnabled,
            _ => false
        };
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(formatter);
        var message = formatter(state, exception);

        switch (logLevel)
        {
            case LogLevel.Trace:
                logger.Logger.Log(typeof(Log4NetLogger), log4net.Core.Level.Trace, message, exception);
                break;
            case LogLevel.Debug:
                logger.Debug(message, exception);
                break;
            case LogLevel.Information:
                logger.Info(message, exception);
                break;
            case LogLevel.Warning:
                logger.Warn(message, exception);
                break;
            case LogLevel.Error:
                logger.Error(message, exception);
                break;
            case LogLevel.Critical:
                logger.Fatal(message, exception);
                break;
        }
    }
}
