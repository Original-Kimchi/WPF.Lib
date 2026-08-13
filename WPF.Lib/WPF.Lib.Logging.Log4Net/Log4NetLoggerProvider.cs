using System.Collections.Concurrent;
using System.Reflection;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.Extensions.Logging;

namespace WPF.Lib.Logging.Log4Net;

public sealed class Log4NetLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, Log4NetLogger> _loggers = new();
    private readonly ILoggerRepository _repository;

    public Log4NetLoggerProvider(Log4NetLoggerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var configPath = Path.GetFullPath(options.ConfigFilePath);
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException("log4net 설정 파일을 찾을 수 없습니다.", configPath);
        }

        Directory.CreateDirectory(options.LogDirectory);
        GlobalContext.Properties["LogDirectory"] = Path.GetFullPath(options.LogDirectory);

        var repositoryName = $"{options.RepositoryName}.{Guid.NewGuid():N}";
        _repository = LogManager.CreateRepository(repositoryName);
        XmlConfigurator.Configure(_repository, new FileInfo(configPath));
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(
            categoryName,
            name => new Log4NetLogger(LogManager.GetLogger(_repository.Name, name)));
    }

    public void Dispose()
    {
        LogManager.ShutdownRepository(_repository.Name);
        _loggers.Clear();
    }
}
