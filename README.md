# WPF.Lib

`WPF.Lib`는 WPF 애플리케이션에서 재사용하는 공통 라이브러리 모음입니다. 각 프로젝트는 독립적으로 참조할 수 있으며 .NET 8을 대상으로 합니다.

## 프로젝트

| 프로젝트 | 대상 프레임워크 | 용도 | 문서 |
|---|---|---|---|
| `WPF.Lib.MVVM` | `net8.0` | 데이터 바인딩을 위한 모델 및 뷰 모델 기반 클래스 | [README](WPF.Lib.MVVM/README.md) |
| `WPF.Lib.Theme` | `net8.0-windows` | 라이트/다크 테마, 공통 컨트롤 스타일, 런타임 테마 전환 | [README](WPF.Lib.Theme/README.md) |
| `WPF.Lib.Controls` | `net8.0-windows` | 입력, 피드백, 이미지 주석 컨트롤과 재사용 가능한 드래그 앤 드롭 동작 | [README](WPF.Lib.Controls/README.md) |

## 프로젝트 참조

저장소 루트에서 필요한 라이브러리만 애플리케이션 프로젝트에 추가합니다.

```powershell
dotnet add <애플리케이션.csproj> reference WPF.Lib/WPF.Lib.MVVM/WPF.Lib.MVVM.csproj
dotnet add <애플리케이션.csproj> reference WPF.Lib/WPF.Lib.Theme/WPF.Lib.Theme.csproj
dotnet add <애플리케이션.csproj> reference WPF.Lib/WPF.Lib.Controls/WPF.Lib.Controls.csproj
```

## 빌드 및 검증

```powershell
dotnet restore WPFControls.UI.sln
dotnet build WPFControls.UI.sln --configuration Debug
dotnet test WPFControls.UI.sln
dotnet format WPFControls.UI.sln --verify-no-changes
```

현재 별도의 테스트 프로젝트는 없습니다. UI와 무관한 동작을 추가할 때는 `WPFControls.UI.Tests`와 같은 형제 테스트 프로젝트에 단위 테스트를 추가합니다.

## 유지보수 원칙

- 재사용 가능한 스타일과 리소스는 애플리케이션이 아니라 해당 라이브러리에 둡니다.
- `LightTheme.xaml`과 `DarkTheme.xaml`에는 동일한 리소스 키를 유지합니다.
- 바인딩 가능한 상태 변경은 `BaseModel.SetProperty`를 사용합니다.
- 새 공개 API 또는 리소스 키를 추가하면 해당 프로젝트의 README도 함께 갱신합니다.
