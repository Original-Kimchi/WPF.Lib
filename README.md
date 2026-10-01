# WPF.Lib

`WPF.Lib`는 .NET 8 애플리케이션에서 재사용하는 공통 라이브러리 모음입니다. UI 독립 프로젝트는 `net8.0`, WPF 리소스나 컨트롤이 필요한 프로젝트는 `net8.0-windows`를 대상으로 합니다.

## 프로젝트 선택

| 프로젝트 | 대상 | 직접 의존성 | 언제 참조하는가 |
|---|---|---|---|
| [`WPF.Lib.Api`](WPF.Lib.Api/README.md) | `net8.0` | Microsoft.Extensions.Http, Logging.Abstractions | JSON 기반 HTTP API 호출과 공통 오류 처리가 필요할 때 |
| [`WPF.Lib.Core`](WPF.Lib.Core/README.md) | `net8.0` | 없음 | 테마·설정·메뉴·대화상자 같은 공통 계약과 모델이 필요할 때 |
| [`WPF.Lib.MVVM`](WPF.Lib.MVVM/README.md) | `net8.0` | 없음 | 변경 알림, `RelayCommand`, 셸 또는 다이얼로그 ViewModel 기반이 필요할 때 |
| [`WPF.Lib.Theme`](WPF.Lib.Theme/README.md) | `net8.0-windows` | `WPF.Lib.MVVM`, MaterialDesignThemes | 라이트·다크 테마와 WPF 기본 컨트롤 스타일을 적용할 때 |
| [`WPF.Lib.Controls`](WPF.Lib.Controls/README.md) | `net8.0-windows` | `WPF.Lib.MVVM` | 공통 입력·피드백·이미지·드래그 앤 드롭 UI가 필요할 때 |
| [`WPF.Lib.Services`](WPF.Lib.Services/README.md) | `net8.0-windows` | Core, Controls, MVVM, Theme | 설정, 메뉴, 테마, 파일·메시지 대화상자 등 공통 WPF 서비스를 일괄 등록할 때 |
| [`WPF.Lib.Logging.Log4Net`](WPF.Lib.Logging.Log4Net/README.md) | `net8.0` | log4net, Microsoft.Extensions.Logging | `ILogger<T>` 로그를 log4net으로 저장할 때 |

프로젝트 참조는 전이되지만, 애플리케이션 코드가 어떤 라이브러리의 공개 타입을 직접 사용한다면 해당 프로젝트를 명시적으로 참조하는 편이 의도를 파악하기 쉽습니다.

## 의존성 방향

```text
애플리케이션 UI
├──> WPF.Lib.Services ──> Core, Controls, MVVM, Theme
├──> WPF.Lib.Core
├──> WPF.Lib.Api
├──> WPF.Lib.Logging.Log4Net
├──> WPF.Lib.Theme ──────> WPF.Lib.MVVM
└──> WPF.Lib.Controls ───> WPF.Lib.MVVM
```

- `Core`와 `MVVM`은 WPF 어셈블리를 참조하지 않습니다.
- `Api`는 WPF에 종속되지 않으며 공통 HTTP 전송과 오류 처리만 담당합니다.
- `Theme`과 `Controls`는 WPF 전용이며 `MVVM`을 사용합니다.
- `Core`는 서비스 계약과 공통 모델을 제공하고, `Services`는 이 저장소에서 공통화된 WPF 구현과 DI 등록을 제공합니다.
- 제품별 저장소, 외부 API, 화면 탐색 정책은 실행 프로젝트 또는 제품별 Infrastructure 프로젝트가 담당합니다.
- 라이브러리에서 실행 프로젝트를 역참조하지 않습니다.

## 프로젝트 참조

다음 명령은 이 라이브러리를 서브모듈로 포함한 상위 `WPFControls.UI` 저장소 루트에서 실행하는 기준입니다. `WPF.Lib`를 독립 저장소로 사용한다면 경로에서 `01.WPF.Lib/` 접두사를 제외합니다.

필요한 프로젝트만 추가합니다.

```powershell
dotnet add <애플리케이션.csproj> reference 01.WPF.Lib/WPF.Lib.Core/WPF.Lib.Core.csproj
dotnet add <애플리케이션.csproj> reference 01.WPF.Lib/WPF.Lib.Api/WPF.Lib.Api.csproj
dotnet add <애플리케이션.csproj> reference 01.WPF.Lib/WPF.Lib.MVVM/WPF.Lib.MVVM.csproj
dotnet add <애플리케이션.csproj> reference 01.WPF.Lib/WPF.Lib.Theme/WPF.Lib.Theme.csproj
dotnet add <애플리케이션.csproj> reference 01.WPF.Lib/WPF.Lib.Controls/WPF.Lib.Controls.csproj
dotnet add <애플리케이션.csproj> reference 01.WPF.Lib/WPF.Lib.Services/WPF.Lib.Services.csproj
dotnet add <애플리케이션.csproj> reference 01.WPF.Lib/WPF.Lib.Logging.Log4Net/WPF.Lib.Logging.Log4Net.csproj
```

최소 조합의 예시는 다음과 같습니다.

| 요구 사항 | 참조 |
|---|---|
| JSON HTTP API 호출 | `WPF.Lib.Api` |
| UI 독립 모델과 ViewModel만 작성 | `WPF.Lib.MVVM` |
| 공통 서비스 계약도 사용 | `WPF.Lib.Core`, `WPF.Lib.MVVM` |
| 공통 WPF 스타일 적용 | 위 항목 + `WPF.Lib.Theme` |
| 사용자 정의 컨트롤 사용 | `WPF.Lib.Controls`, 보통 `WPF.Lib.Theme`도 함께 사용 |
| 공통 WPF 애플리케이션 서비스 사용 | `WPF.Lib.Services` 및 전이 참조 |
| 파일 로그 사용 | `WPF.Lib.Logging.Log4Net` |

전체 샘플 구성과 새 솔루션의 권장 구조는 [라이브러리 도입 가이드](../docs/library-adoption-guide.md)를 참고합니다.

## 공용 코드의 경계

`WPF.Lib`에는 둘 이상의 제품에서 같은 의미와 동작으로 재사용되는 코드만 둡니다.

- 제품의 업무 엔터티와 유스케이스는 제품별 `MyApp.Core` 또는 `MyApp.Application`에 둡니다.
- 범용 설정, 테마, 파일·메시지 대화상자는 `WPF.Lib.Services`에 둡니다.
- 제품별 파일 저장, DB, 네트워크와 화면 탐색 정책은 제품별 Infrastructure 또는 실행 프로젝트에 둡니다.
- 범용 컨트롤의 의존성 속성과 모델은 `WPF.Lib.Controls`에 둡니다.
- 테마 키와 암시적 스타일은 `WPF.Lib.Theme`에 둡니다.
- 특정 화면만 사용하는 ViewModel이나 변환기는 실행 프로젝트에 둡니다.

## 빌드 및 검증

```powershell
dotnet restore WPFControls.UI.sln
dotnet build WPFControls.UI.sln --configuration Debug
dotnet test WPFControls.UI.sln
dotnet format WPFControls.UI.sln --verify-no-changes
```

현재 별도 테스트 프로젝트는 없습니다. UI와 무관한 동작을 추가할 때는 대상 라이브러리와 대응하는 테스트 프로젝트를 만들고, WPF 템플릿 변경은 라이트·다크 테마의 기본, hover, focus, 선택, 비활성, 팝업 상태를 확인합니다.

리소스 수명, 타이머 해제, 싱글턴 ViewModel과 네트워크·설정 안전성 기준은 [라이브러리 수명·메모리·안전성](../docs/library-lifecycle-and-safety.md)을 함께 확인합니다.
