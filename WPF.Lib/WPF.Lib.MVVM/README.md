# WPF.Lib.MVVM

WPF 데이터 바인딩에 필요한 최소 기반 클래스를 제공하는 .NET 8 클래스 라이브러리입니다. WPF 어셈블리에 직접 의존하지 않으므로 UI와 무관한 모델 및 뷰 모델에서도 사용할 수 있습니다.

## 제공 API

### `BaseModel`

`INotifyPropertyChanged`를 구현하는 추상 클래스입니다.

- `PropertyChanged`: 바인딩 대상 속성의 변경 알림 이벤트
- `OnPropertyChanged(string? propertyName = null)`: 지정한 속성의 변경 알림 발생
- `SetProperty<T>(ref T field, T value, string? propertyName = null)`: 값이 실제로 바뀐 경우에만 필드를 갱신하고 알림 발생

`OnPropertyChanged`와 `SetProperty`는 `CallerMemberName`을 사용하므로 일반적인 속성 setter에서는 속성 이름을 생략할 수 있습니다. `SetProperty`는 값이 변경되면 `true`, 동일하면 `false`를 반환합니다.

### `BaseViewModel`

`BaseModel`을 상속하는 뷰 모델용 추상 클래스입니다. 현재 추가 동작은 없으며 모델과 뷰 모델의 역할을 타입 수준에서 구분합니다.

## 참조

저장소 루트에서 다음 명령을 실행합니다.

```powershell
dotnet add <애플리케이션.csproj> reference WPF.Lib/WPF.Lib.MVVM/WPF.Lib.MVVM.csproj
```

## 사용 예제

```csharp
using WPF.Lib.MVVM;

public sealed class CustomerViewModel : BaseViewModel
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}
```

값 변경 뒤 추가 작업이 필요하면 반환값을 사용합니다.

```csharp
if (SetProperty(ref _name, value))
{
    OnPropertyChanged(nameof(DisplayName));
}
```

## 파일 구조

```text
WPF.Lib.MVVM/
├─ BaseModel.cs
├─ BaseViewModel.cs
└─ WPF.Lib.MVVM.csproj
```

## 확장 지침

- UI와 무관한 상태 및 로직만 이 프로젝트에 둡니다.
- 속성 setter에서 직접 `PropertyChanged`를 발생시키기보다 `SetProperty`를 사용합니다.
- 계산 속성처럼 함께 바뀌는 속성은 `OnPropertyChanged`로 명시합니다.
- 명령, 검증 등 공통 기능을 추가할 때는 UI 독립성을 유지하고 단위 테스트를 함께 추가합니다.
