using Microsoft.Extensions.Logging;
using WPF.Lib.Core.Abstractions;

namespace WPFControls.UI.Services;

public sealed class ExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(ILogger<ExceptionHandler> logger)
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
