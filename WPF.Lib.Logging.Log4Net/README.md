# WPF.Lib.Logging.Log4Net

`Microsoft.Extensions.Logging`의 `ILogger<T>` 호출을 log4net으로 전달하는 재사용 가능한 provider입니다.

## 등록

```csharp
services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddLog4Net(options =>
    {
        options.ConfigFilePath = Path.Combine(AppContext.BaseDirectory, "log4net.config");
        options.LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ApplicationName",
            "Logs");
    });
});
```

애플리케이션 코드에서는 log4net의 `ILog` 대신 `ILogger<T>`를 생성자로 주입받아 사용합니다.

```csharp
public MainViewModel(ILogger<MainViewModel> logger)
{
    _logger = logger;
}
```

설정 파일의 파일 경로에는 provider가 등록하는 `%property{LogDirectory}`를 사용할 수 있습니다.
