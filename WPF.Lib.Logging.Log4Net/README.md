# WPF.Lib.Logging.Log4Net

`Microsoft.Extensions.Logging`의 `ILogger<T>` 호출을 log4net으로 전달하는 `net8.0` provider입니다. 애플리케이션 코드는 log4net의 `ILog`에 직접 의존하지 않습니다.

## 참조

```powershell
dotnet add <애플리케이션.csproj> reference WPF.Lib/WPF.Lib.Logging.Log4Net/WPF.Lib.Logging.Log4Net.csproj
```

## 설정 파일 배포

실행 프로젝트에 `log4net.config`를 두고 출력 디렉터리에 복사합니다.

```xml
<ItemGroup>
  <None Update="log4net.config">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

설정 파일에서 provider가 등록하는 로그 폴더를 사용할 수 있습니다.

```xml
<file value="%property{LogDirectory}/application.log" />
```

## 등록

```csharp
services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.SetMinimumLevel(LogLevel.Information);
    builder.AddLog4Net(options =>
    {
        options.ConfigFilePath = Path.Combine(
            AppContext.BaseDirectory,
            "log4net.config");
        options.LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ApplicationName",
            "Logs");
        options.RepositoryName = "ApplicationName";
    });
});
```

| 옵션 | 기본값 | 설명 |
|---|---|---|
| `ConfigFilePath` | `log4net.config` | log4net XML 설정 파일 경로 |
| `LogDirectory` | `%LocalAppData%/WPFControls.UI/Logs` | 생성할 로그 디렉터리와 `LogDirectory` 전역 속성 값 |
| `RepositoryName` | `WPF.Lib.Logging.Log4Net` | 격리된 log4net repository 이름의 접두사 |

설정 파일이 없으면 provider 생성 시 `FileNotFoundException`이 발생합니다. `ConfigFilePath`는 출력 디렉터리 기준의 절대 경로로 구성하는 것이 안전합니다.

## 사용

```csharp
public sealed class MainViewModel(ILogger<MainViewModel> logger)
{
    public void Save()
    {
        logger.LogInformation("저장을 시작합니다.");
    }
}
```

DI 컨테이너를 종료할 때 `ServiceProvider.Dispose()`를 호출하면 provider가 repository를 종료하고 logger 캐시를 정리합니다.
