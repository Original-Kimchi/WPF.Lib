using Microsoft.Extensions.Logging;
using WPF.Lib.Core.Abstractions;

namespace WPF.Lib.Services;

public sealed class LoggingExceptionHandler : IExceptionHandler
{
    private readonly ILogger<LoggingExceptionHandler> _logger;

    public LoggingExceptionHandler(ILogger<LoggingExceptionHandler> logger)
    {
        _logger = logger;
    }

    public void Handle(Exception exception, string source, bool isTerminating = false)
    {
        if (isTerminating)
        {
            _logger.LogCritical(
                exception,
                "처리되지 않은 예외가 발생했습니다. Source: {Source}, IsTerminating: {IsTerminating}",
                source,
                isTerminating);
            return;
        }

        _logger.LogError(exception, "처리되지 않은 예외가 발생했습니다. Source: {Source}", source);
    }
}
