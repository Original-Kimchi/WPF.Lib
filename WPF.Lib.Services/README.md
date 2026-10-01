# WPF.Lib.Services

`WPF.Lib.Core`의 공통 계약을 WPF로 구현하고 DI 등록을 제공하는 `net8.0-windows` 라이브러리입니다. 제품 실행 프로젝트는 메뉴와 애플리케이션 이름만 구성하고, 반복적인 WPF 서비스 구현을 재사용할 수 있습니다.

## 참조와 등록

다음 명령은 상위 `WPFControls.UI` 저장소 루트 기준입니다. `WPF.Lib` 독립 저장소에서는 `01.WPF.Lib/` 접두사를 제외합니다.

```powershell
dotnet add <애플리케이션.csproj> reference 01.WPF.Lib/WPF.Lib.Services/WPF.Lib.Services.csproj
```

```csharp
using WPF.Lib.Core.Models;
using WPF.Lib.Services;

services.AddWpfServices(options =>
{
    options.ApplicationName = "MyApp";
    options.SettingsFileName = "settings.json";
    options.Menus.Add(new MenuItemDefinition(
        Id: "home",
        Title: "Home",
        Route: "home",
        Icon: "HomeOutline",
        Order: 0));
});
```

`ApplicationName`은 필수입니다. 설정은 기본적으로 `%LocalAppData%/<ApplicationName>/<SettingsFileName>`에 저장됩니다. 메뉴 정의는 제품마다 다르므로 실행 프로젝트에서 등록합니다.

## 등록되는 서비스

| 계약 | 구현 | 역할 |
|---|---|---|
| `IMenuService` | `MenuService` | 등록된 메뉴를 순서대로 제공 |
| `IDialogService` | `DialogService` | 공통 메시지·확인 다이얼로그 표시 |
| `IMessageBoxService` | `MessageBoxService` | 테마 메시지 박스 표시와 결과 반환 |
| `IFileDialogService` | `FileDialogService` | 파일 열기·저장 경로 선택 |
| `IThemeService` | `ThemeService` | 라이트·다크 테마 전환 |
| `ISettingsService` | `SettingsService` | 공통 `AppSettings` JSON 저장·복원 |
| `IAppLifetimeService` | `AppLifetimeService` | WPF 애플리케이션 종료 요청 |
| `IExceptionHandler` | `LoggingExceptionHandler` | 처리되지 않은 예외 로깅 |

모든 구현은 Singleton으로 등록됩니다. 애플리케이션 종료 시 루트 `ServiceProvider.Dispose()`를 호출해야 DI가 추적하는 `IDisposable` 객체를 정리할 수 있습니다.

`INavigationService`는 제품별 화면 생성과 탐색 기록 정책이 필요하므로 `AddWpfServices`에서 등록하지 않습니다.
실행 프로젝트에서 `INavigationService` 구현을 만든 뒤 원하는 수명으로 별도 등록합니다.

## IMessageBoxService 위치

대부분의 계약은 WPF 타입을 노출하지 않으므로 `WPF.Lib.Core/Abstractions`에 있습니다. `IMessageBoxService`는 `System.Windows.MessageBoxButton`, `MessageBoxResult`와 `WPF.Lib.Controls.Models.MessageBoxType`을 공개 API로 사용하므로 WPF 독립적인 Core에 둘 수 없습니다. 따라서 `WPF.Lib.Services/Abstractions`에 둡니다.

Core로 옮기려면 WPF와 무관한 버튼·결과·심각도 모델을 Core에 정의하고 WPF 구현에서 변환해야 합니다.

## 공통 구현과 제품 구현의 경계

다음은 이 라이브러리에 둡니다.

- 여러 WPF 제품에서 동일하게 동작하는 창 수명, 테마, 파일 대화상자
- 공통 AppSettings 저장과 전역 예외 로깅
- 공통 메시지 및 확인 다이얼로그
- 메뉴 정렬과 제공 로직

다음은 제품 프로젝트에 둡니다.

- 실제 메뉴 항목과 Route별 ViewModel 해석
- 제품별 설정 모델과 업무 데이터 저장
- 제품 전용 다이얼로그와 ViewModel
- 외부 API, DB, 장비 통신 구현

## 스레드와 수명 주의사항

- 다이얼로그, 파일 대화상자, 테마 서비스는 UI 스레드에서 호출합니다.
- `LoadAsync`와 `SaveAsync`는 동시에 호출하지 않습니다.
- 종료 경로에서 비동기 메서드를 동기 대기할 때 내부 구현이 UI 컨텍스트를 다시 기다리지 않도록 주의합니다.
- 메뉴 옵션은 서비스 등록이 끝난 뒤 변경하지 않습니다. `MenuService`는 생성 시 메뉴 스냅샷을 만듭니다.
- 현재 `ISettingsService`는 공통 `AppSettings`에 고정되어 있습니다. 제품별 설정이 필요하면 별도 `ISettingsStore<TSettings>`를 제품 Infrastructure에 둡니다.

상세 점검표는 [라이브러리 수명·메모리·안전성](../../docs/library-lifecycle-and-safety.md)을 참고합니다.
