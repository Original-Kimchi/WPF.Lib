using Microsoft.Extensions.Logging;

namespace WPF.Lib.Logging.Log4Net;

public static class Log4NetLoggingBuilderExtensions
{
    public static ILoggingBuilder AddLog4Net(
        this ILoggingBuilder builder,
        Action<Log4NetLoggerOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new Log4NetLoggerOptions();
        configure?.Invoke(options);
        builder.AddProvider(new Log4NetLoggerProvider(options));
        return builder;
    }
}
