# WPF.Lib 도입 가이드

이 문서는 `WPF.Lib` 프로젝트를 다른 .NET 8 WPF 솔루션에서 사용할 때의 프로젝트 구성, 참조 방향, 앱 초기 설정을 설명합니다. 현재 샘플인 `WPFControls.UI`의 구조를 출발점으로 하되, 제품별 업무 코드는 공용 라이브러리와 분리합니다.

## 1. 권장 솔루션 구조

작은 애플리케이션은 실행 프로젝트 하나와 `WPF.Lib` 참조만으로 시작해도 됩니다. 저장소, 외부 API, 업무 규칙이 늘어나면 다음처럼 제품별 프로젝트를 분리합니다.

```text
MyApp/
├─ MyApp.sln
├─ src/
│  ├─ MyApp/                       # WPF 실행 프로젝트와 Composition Root
│  │  ├─ Views/
│  │  ├─ ViewModels/
│  │  ├─ Services/                 # 작은 앱의 WPF 전용 구현
│  │  ├─ Converters/
│  │  ├─ App.xaml
│  │  └─ App.xaml.cs
│  ├─ MyApp.Core/                  # 제품의 도메인 모델과 UI 독립 계약
│  └─ MyApp.Infrastructure/        # 파일, DB, 네트워크 등 계약 구현
├─ tests/
│  ├─ MyApp.Core.Tests/
│  └─ MyApp.Infrastructure.Tests/
└─ WPF.Lib/                        # 공통 저장소를 함께 둘 때
   ├─ WPF.Lib.Core/
   ├─ WPF.Lib.MVVM/
   ├─ WPF.Lib.Theme/
   ├─ WPF.Lib.Controls/
   └─ WPF.Lib.Logging.Log4Net/
```

`WPF.Lib.Core`는 여러 제품에서 공통인 셸 수준 계약을 제공합니다. 한 제품에만 의미가 있는 주문, 장비, 검사 결과 같은 모델은 `MyApp.Core`에 둡니다.

## 2. 권장 참조 방향

```text
MyApp (WPF)
├─> MyApp.Core
├─> MyApp.Infrastructure ──> MyApp.Core
├─> WPF.Lib.Core
├─> WPF.Lib.MVVM
├─> WPF.Lib.Theme ─────────> WPF.Lib.MVVM
├─> WPF.Lib.Controls ──────> WPF.Lib.MVVM
└─> WPF.Lib.Logging.Log4Net
```

참조 방향을 유지하기 위한 기준은 다음과 같습니다.

- `Core` 프로젝트에는 `Window`, `MessageBox`, `OpenFileDialog`, `Dispatcher` 같은 WPF 타입을 넣지 않습니다.
- ViewModel은 서비스 인터페이스를 생성자로 받고, 구현체를 직접 생성하지 않습니다.
- View는 ViewModel에 바인딩하고, 앱 시작과 객체 조립은 `App.xaml.cs` 한 곳에서 담당합니다.
- Infrastructure는 Core를 참조할 수 있지만 Core가 Infrastructure를 참조하지 않습니다.
- 공용 라이브러리는 `MyApp` 같은 제품 실행 프로젝트를 참조하지 않습니다.

## 3. 프로젝트 생성과 참조

새 WPF 앱과 선택적 계층을 만든 뒤 솔루션에 추가합니다.

```powershell
dotnet new sln --name MyApp
dotnet new wpf --name MyApp --framework net8.0
dotnet new classlib --name MyApp.Core --framework net8.0
dotnet new classlib --name MyApp.Infrastructure --framework net8.0

dotnet sln MyApp.sln add MyApp/MyApp.csproj
dotnet sln MyApp.sln add MyApp.Core/MyApp.Core.csproj
dotnet sln MyApp.sln add MyApp.Infrastructure/MyApp.Infrastructure.csproj
```

공용 라이브러리가 같은 저장소에 있다는 기준의 예시입니다. 실제 상대 경로는 저장소 구조에 맞게 조정합니다.

```powershell
dotnet add MyApp/MyApp.csproj reference WPF.Lib/WPF.Lib.Core/WPF.Lib.Core.csproj
dotnet add MyApp/MyApp.csproj reference WPF.Lib/WPF.Lib.MVVM/WPF.Lib.MVVM.csproj
dotnet add MyApp/MyApp.csproj reference WPF.Lib/WPF.Lib.Theme/WPF.Lib.Theme.csproj
dotnet add MyApp/MyApp.csproj reference WPF.Lib/WPF.Lib.Controls/WPF.Lib.Controls.csproj
dotnet add MyApp/MyApp.csproj reference WPF.Lib/WPF.Lib.Logging.Log4Net/WPF.Lib.Logging.Log4Net.csproj
```

`MyApp.Core`와 `MyApp.Infrastructure`를 분리했다면 다음 참조도 추가합니다.

```powershell
dotnet add MyApp/MyApp.csproj reference MyApp.Core/MyApp.Core.csproj
dotnet add MyApp/MyApp.csproj reference MyApp.Infrastructure/MyApp.Infrastructure.csproj
dotnet add MyApp.Infrastructure/MyApp.Infrastructure.csproj reference MyApp.Core/MyApp.Core.csproj
```

실행 프로젝트에서 DI와 로깅 구성 API를 직접 사용하므로 패키지도 명시적으로 추가합니다. 버전은 솔루션의 .NET 및 패키지 정책에 맞춰 통일합니다.

```powershell
dotnet add MyApp/MyApp.csproj package Microsoft.Extensions.DependencyInjection --version 8.0.1
dotnet add MyApp/MyApp.csproj package Microsoft.Extensions.Logging --version 8.0.0
```

## 4. 테마와 컨트롤 초기화

`App.xaml`에는 시작 테마 하나를 병합합니다. 테마 사전이 모든 공통 컨트롤 스타일을 내부에서 불러옵니다.

```xaml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="/WPF.Lib.Theme;component/Theme/LightTheme.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

사용자 정의 컨트롤을 사용하는 View에는 네임스페이스를 선언합니다.

```xaml
xmlns:controls="clr-namespace:WPF.Lib.Controls.Controls;assembly=WPF.Lib.Controls"
```

테마가 런타임에 바뀌어야 하는 앱 리소스는 `StaticResource`가 아니라 `DynamicResource`로 참조합니다.

```xaml
<Grid Background="{DynamicResource BackgroundBrush}">
    <TextBlock Foreground="{DynamicResource ForegroundBrush}"
               Text="Hello" />
</Grid>
```

## 5. DI를 Composition Root에 구성

생성자 주입이 필요한 `MainWindow`를 WPF가 직접 만들지 않도록 `App.xaml`에서 `StartupUri`를 제거합니다. 실행 프로젝트에 `Microsoft.Extensions.DependencyInjection`을 추가하고 `App.xaml.cs`에서 컨테이너를 한 번 구성합니다.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WPF.Lib.Controls.Abstractions;
using WPF.Lib.Controls.Services;
using WPF.Lib.Core.Abstractions;
using WPF.Lib.Logging.Log4Net;

private static void ConfigureServices(IServiceCollection services)
{
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
                "MyApp",
                "Logs");
        });
    });

    services.AddSingleton<IToastService, ToastService>();
    services.AddSingleton<ISettingsService, SettingsService>();

    services.AddSingleton<MainViewModel>();
    services.AddSingleton<MainWindow>();
}
```

`ISettingsService` 구현은 실행 프로젝트나 Infrastructure 프로젝트에 둡니다. 서비스 수명은 앱 전체에서 상태를 공유하면 Singleton, 창을 열 때마다 새 상태가 필요하면 Transient를 선택합니다. WPF에는 웹 요청과 같은 자동 Scope가 없으므로 Scoped 수명은 작업 범위를 직접 만들 때만 사용합니다.

## 6. ViewModel과 View 연결

ViewModel은 `BaseViewModel`과 `RelayCommand`를 사용하고 WPF 창을 직접 생성하지 않습니다.

```csharp
using WPF.Lib.MVVM;
using WPF.Lib.MVVM.Commands;

public sealed class MainViewModel : BaseViewModel
{
    private string _query = string.Empty;

    public MainViewModel()
    {
        SearchCommand = new RelayCommand(Search, CanSearch);
    }

    public string Query
    {
        get => _query;
        set
        {
            if (SetProperty(ref _query, value))
            {
                SearchCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public RelayCommand SearchCommand { get; }

    private bool CanSearch() => !string.IsNullOrWhiteSpace(Query);

    private void Search()
    {
        // 유스케이스 또는 서비스를 호출합니다.
    }
}
```

Window는 주입받은 ViewModel을 `DataContext`로 연결합니다.

```csharp
public MainWindow(MainViewModel viewModel)
{
    InitializeComponent();
    DataContext = viewModel;
}
```

## 7. log4net 설정 파일 배포

실행 프로젝트의 `log4net.config`가 출력 폴더에 복사되도록 설정합니다.

```xml
<ItemGroup>
  <None Update="log4net.config">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

설정 파일의 로그 경로에는 provider가 등록하는 `%property{LogDirectory}`를 사용할 수 있습니다. 파일이 출력 폴더에 없으면 provider 생성 시 `FileNotFoundException`이 발생합니다.

## 8. 기능 배치 판단표

| 추가하려는 코드 | 권장 위치 |
|---|---|
| 여러 제품이 공유하는 서비스 계약과 단순 DTO | `WPF.Lib.Core` |
| 한 제품의 도메인 모델과 유스케이스 계약 | `MyApp.Core` 또는 `MyApp.Application` |
| 변경 알림이나 범용 명령 기반 기능 | `WPF.Lib.MVVM` |
| 여러 화면·제품에서 재사용하는 WPF 컨트롤 | `WPF.Lib.Controls` |
| 공통 색상, 브러시, 기본 컨트롤 스타일 | `WPF.Lib.Theme` |
| 파일·DB·HTTP·제품별 설정 저장 구현 | `MyApp.Infrastructure` |
| 특정 화면의 View, ViewModel, Converter | `MyApp` |
| 앱 시작, DI 등록, 전역 예외 연결 | `MyApp/App.xaml.cs` |

## 9. 도입 확인 목록

- 실행 프로젝트가 `net8.0-windows`와 `<UseWPF>true</UseWPF>`를 사용하는가
- `App.xaml`에 시작 테마가 정확히 하나 병합되어 있는가
- `StartupUri` 없이 DI에서 `MainWindow`를 생성하는가
- ViewModel이 WPF 타입이나 구체 서비스를 직접 생성하지 않는가
- `log4net.config`가 출력 디렉터리로 복사되는가
- 실행 종료 시 `ServiceProvider.Dispose()`를 호출하는가
- 라이트·다크 테마에서 hover, focus, 선택, 비활성, 팝업 상태를 확인했는가
- UI 독립 로직에 단위 테스트를 추가했는가

세부 API는 각 프로젝트의 README를 확인합니다.
