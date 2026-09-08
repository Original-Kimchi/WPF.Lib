# WPF.Lib.Core

WPF 애플리케이션에서 공통으로 사용할 UI 독립 계약과 모델을 제공하는 `net8.0` 라이브러리입니다. WPF 구체 타입을 참조하지 않으므로 ViewModel, Infrastructure, 테스트 프로젝트에서 함께 사용할 수 있습니다.

## 참조

```powershell
dotnet add <프로젝트.csproj> reference WPF.Lib/WPF.Lib.Core/WPF.Lib.Core.csproj
```

## 제공 계약

| 인터페이스 | 역할 |
|---|---|
| `IAppLifetimeService` | UI 타입을 노출하지 않고 앱 종료 요청 |
| `IDialogService` | 메시지 및 확인 대화상자 표시 |
| `IExceptionHandler` | 전역 또는 작업 예외 처리 |
| `IFileDialogService` | 파일 열기·저장 경로 선택 |
| `IMenuService` | 셸 메뉴 정의 제공 |
| `INavigationService` | ViewModel 형식 기반 화면 이동과 뒤로 가기 |
| `ISettingsService` | 현재 설정의 로드와 저장 |
| `IThemeService` | 현재 테마 조회와 변경 |

`INavigationService`는 애플리케이션마다 화면 구성과 수명 정책이 다르므로 계약만 제공합니다.
`WPF.Lib.Services.AddWpfServices`는 이 인터페이스를 등록하지 않으며, 실행 프로젝트에서 구현체를 선택해 별도로 등록해야 합니다.

## 제공 모델

| 형식 | 역할 |
|---|---|
| `ApplicationTheme` | `Light`, `Dark` 테마 값 |
| `AppSettings` | 테마와 창 위치·크기·최대화 상태 |
| `FileDialogOptions` | 제목, 필터, 초기 폴더, 기본 파일명 |
| `MenuItemDefinition` | ID, 제목, 경로, 아이콘, 순서, 자식 메뉴 |

## 구현 원칙

이 프로젝트에는 WPF 타입에 독립적인 계약만 둡니다. 여러 제품이 함께 쓰는 기본 WPF 구현은 `WPF.Lib.Services`에, 제품 도메인에 결합된 구현은 실행 프로젝트 또는 제품별 Infrastructure에 둡니다.

Composition Root에서는 `WPF.Lib.Services`의 등록 확장을 사용해 기본 구현이 제공되는 공통 계약을 연결합니다.

```csharp
using WPF.Lib.Services;

services.AddWpfServices(options =>
{
    options.ApplicationName = "MyApp";
});
```

## 확장 기준

- 여러 제품에서 동일한 의미로 재사용되는 UI 독립 계약과 DTO만 추가합니다.
- `Window`, `MessageBox`, `Dispatcher`, 파일 대화상자 구현 등 WPF 타입을 노출하지 않습니다.
- 한 제품에만 필요한 업무 모델은 해당 제품의 `MyApp.Core` 또는 `MyApp.Application`에 둡니다.
- 계약을 변경하면 모든 구현체와 DI 등록, 관련 문서를 함께 확인합니다.

기본 구현은 [WPF.Lib.Services README](../WPF.Lib.Services/README.md), 다른 솔루션에서의 계층 구성은 [WPF.Lib 도입 가이드](../../docs/library-adoption-guide.md)를 참고합니다.
