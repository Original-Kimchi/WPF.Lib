# WPFControls.UI

.NET 8 WPF에서 공통 UI, MVVM, 테마, 로깅, 애플리케이션 서비스 분리를 시험하고 재사용하기 위한 샘플 솔루션입니다. `WPFControls.UI`는 실행 가능한 예제 애플리케이션이고, `WPF.Lib` 아래 프로젝트는 다른 WPF 솔루션에서도 선택적으로 참조할 수 있는 라이브러리입니다.

## 솔루션 구성

| 경로 | 역할 |
|---|---|
| `WPFControls.UI/` | 앱 시작, DI 구성, View·ViewModel, 서비스 구현을 포함한 샘플 애플리케이션 |
| `WPF.Lib/WPF.Lib.Core/` | UI 기술에 독립적인 애플리케이션 계약과 공통 모델 |
| `WPF.Lib/WPF.Lib.MVVM/` | 변경 알림, 명령, 셸 및 다이얼로그 ViewModel 기반 클래스 |
| `WPF.Lib/WPF.Lib.Theme/` | 라이트·다크 리소스, WPF 기본 컨트롤 스타일, 다이얼로그 기반 View |
| `WPF.Lib/WPF.Lib.Controls/` | 입력, 피드백, 이미지 뷰어, 드래그 앤 드롭 컨트롤 |
| `WPF.Lib/WPF.Lib.Logging.Log4Net/` | `Microsoft.Extensions.Logging`용 log4net provider |
| `docs/` | 솔루션 구성과 다른 프로젝트에서의 도입 가이드 |

## 시작하기

Windows에서 저장소 루트를 기준으로 실행합니다.

```powershell
dotnet restore WPFControls.UI.sln
dotnet build WPFControls.UI.sln --configuration Debug
dotnet run --project WPFControls.UI/WPFControls.UI.csproj
```

## 문서

- [다른 프로젝트에서 라이브러리 도입하기](docs/library-adoption-guide.md)
- [의존성 주입 구성](docs/dependency-injection.md)
- [WPF.Lib 프로젝트별 안내](WPF.Lib/README.md)
- [컨트롤 사용법](WPF.Lib/WPF.Lib.Controls/README.md)
- [테마 사용법](WPF.Lib/WPF.Lib.Theme/README.md)

각 라이브러리는 필요한 것만 참조할 수 있습니다. 새 애플리케이션의 권장 폴더와 프로젝트 구성, 참조 방향, `App.xaml` 및 DI 초기 설정은 도입 가이드부터 확인합니다.
