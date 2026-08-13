# Dependency Injection 구성

## 개요

`WPFControls.UI`는 `Microsoft.Extensions.DependencyInjection`을 사용합니다. 애플리케이션의 Composition Root는 `App.xaml.cs`이며, 서비스·ViewModel·Window의 생성과 연결을 이곳에서 담당합니다.

`App.xaml`에는 `StartupUri`를 지정하지 않습니다. WPF가 `MainWindow`를 직접 생성하면 생성자 의존성을 주입할 수 없기 때문에, `App.OnStartup`에서 DI 컨테이너를 구성한 뒤 `MainWindow`를 가져와 표시합니다.

## 현재 등록 구성

```csharp
services.AddSingleton<IMenuService, MenuService>();
services.AddSingleton<IDialogService, DialogService>();
services.AddSingleton<IThemeService, ThemeService>();

services.AddSingleton<MainViewModel>();
services.AddSingleton<ShellViewModel>();

services.AddSingleton<MainWindow>();
```

현재 구성요소는 애플리케이션이 실행되는 동안 동일한 상태를 유지하므로 모두 Singleton으로 등록되어 있습니다.

| 등록 | 역할 |
| --- | --- |
| `IMenuService` | 표시할 메뉴 정의 제공 |
| `IDialogService` | WPF 다이얼로그 생성 및 표시 |
| `IThemeService` | 라이트·다크 테마 적용 |
| `MainViewModel` | Main 화면의 데이터와 명령 관리 |
| `ShellViewModel` | 메뉴 선택과 현재 화면 관리 |
| `MainWindow` | 애플리케이션 셸 Window |

## 생성 흐름

```text
App.OnStartup
  -> ServiceCollection 등록
  -> ServiceProvider 생성
  -> MainWindow 요청
  -> ShellViewModel 자동 주입
  -> MainViewModel 및 서비스 자동 주입
  -> MainWindow 표시
```

`MainWindow`는 구체 서비스를 직접 생성하지 않고 필요한 ViewModel을 생성자로 받습니다.

```csharp
public MainWindow(ShellViewModel viewModel)
{
    InitializeComponent();
    DataContext = viewModel;
}
```

## 서비스 추가 방법

먼저 `WPF.Lib.Core`에 UI 기술과 무관한 인터페이스를 정의합니다.

```csharp
public interface ISettingsService
{
    Task SaveAsync(CancellationToken cancellationToken = default);
}
```

`WPFControls.UI` 또는 Infrastructure 프로젝트에 WPF 전용 구현체를 작성하고 `ConfigureServices`에 등록합니다.

```csharp
services.AddSingleton<ISettingsService, SettingsService>();
```

사용하는 ViewModel에서는 구체 클래스를 생성하지 않고 생성자로 인터페이스를 받습니다.

```csharp
public SettingsViewModel(ISettingsService settingsService)
{
    _settingsService = settingsService;
}
```

## 수명 선택 기준

- `Singleton`: 앱 전체에서 하나의 상태를 공유하는 메뉴, 테마, 셸 ViewModel
- `Transient`: 열 때마다 새 인스턴스가 필요한 화면 또는 작업 ViewModel
- `Scoped`: WPF에는 자동 요청 Scope가 없으므로 명확한 작업 범위를 직접 생성할 때만 사용

상태가 있는 화면을 여러 번 독립적으로 열어야 한다면 해당 ViewModel과 Window는 `AddTransient`로 등록합니다.

## 주의사항

- ViewModel에서 `new MenuService()`처럼 구체 서비스를 직접 생성하지 않습니다.
- ViewModel이 `Window`, `MessageBox` 등 WPF 타입을 직접 사용하지 않도록 서비스 인터페이스를 이용합니다.
- `BuildServiceProvider`는 `App.OnStartup`에서 한 번만 호출합니다.
- 앱 종료 시 `ServiceProvider.Dispose()`를 호출해 `IDisposable` 서비스가 정리되도록 합니다.
