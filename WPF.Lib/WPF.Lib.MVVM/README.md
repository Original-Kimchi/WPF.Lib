# WPF.Lib.MVVM

데이터 바인딩에 필요한 변경 알림, 명령, 셸 및 다이얼로그 ViewModel 기반을 제공하는 `net8.0` 라이브러리입니다. WPF 어셈블리에 직접 의존하지 않아 UI 독립 ViewModel과 단위 테스트에서 사용할 수 있습니다.

## 참조

```powershell
dotnet add <프로젝트.csproj> reference WPF.Lib/WPF.Lib.MVVM/WPF.Lib.MVVM.csproj
```

## 제공 API

| 형식 | 역할 |
|---|---|
| `BaseModel` | `INotifyPropertyChanged`, `OnPropertyChanged`, `SetProperty` 제공 |
| `BaseViewModel` | 모델과 ViewModel 역할을 구분하는 기본 형식 |
| `BaseMainViewModel<TMenu>` | 메뉴 선택과 현재 화면 ViewModel 전환의 공통 흐름 |
| `RelayCommand` | 동기 `ICommand`, 실행 가능 조건과 상태 갱신 제공 |
| `BaseDialogViewModel` | 제목, 버튼, 확인 가능 조건, 닫기 요청 제공 |
| `DialogButtonMode` | 버튼 없음, 확인, 확인·취소 구성 |
| `DialogOutcome` | 확인 또는 취소 결과 |

## 변경 알림

`SetProperty`는 값이 실제로 바뀐 경우에만 필드를 갱신하고 알림을 발생시킵니다. 변경되면 `true`, 동일하면 `false`를 반환합니다.

```csharp
public sealed class CustomerViewModel : BaseViewModel
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public string DisplayName => $"고객: {Name}";
}
```

## 명령

`RelayCommand`는 매개변수 없는 대리자와 `object?` 매개변수 대리자를 모두 지원합니다. 실행 가능 조건에 영향을 주는 상태가 바뀌면 `NotifyCanExecuteChanged`를 호출합니다.

```csharp
public RelayCommand SaveCommand { get; }

public EditorViewModel()
{
    SaveCommand = new RelayCommand(Save, () => HasChanges);
}

private void OnHasChangesChanged()
{
    SaveCommand.NotifyCanExecuteChanged();
}
```

현재 `RelayCommand`는 동기 명령입니다. 장시간 I/O는 서비스의 비동기 메서드와 별도의 중복 실행 방지 상태를 사용하거나, 공용 비동기 명령을 추가한 뒤 사용합니다.

## 셸 ViewModel

`BaseMainViewModel<TMenu>`는 `SelectedMenu`가 바뀌면 `ResolveViewModel`을 호출해 `CurrentViewModel`을 교체합니다. 파생 클래스 생성이 끝난 뒤 `SelectInitialMenu`를 호출해야 첫 메뉴가 선택됩니다.

```csharp
public sealed class ShellViewModel : BaseMainViewModel<MenuItemDefinition>
{
    public ShellViewModel(IMenuService menuService)
        : base(menuService.GetMenus())
    {
        SelectInitialMenu();
    }

    protected override object? ResolveViewModel(MenuItemDefinition menu)
    {
        return menu.Route switch
        {
            "home" => new HomeViewModel(),
            _ => null
        };
    }
}
```

실제 앱에서는 `ResolveViewModel` 안에서 ViewModel을 직접 생성하기보다 DI로 주입받은 인스턴스나 탐색 서비스를 사용하는 것이 좋습니다.

## 다이얼로그 ViewModel

`BaseDialogViewModel`을 상속해 확인 가능 조건과 결과를 정의합니다. 입력 상태가 바뀌면 `RefreshConfirmCommand`를 호출합니다. View는 `CloseRequested` 이벤트를 받아 창을 닫고 `DialogOutcome`을 반환합니다.

```csharp
public sealed class RenameDialogViewModel : BaseDialogViewModel
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                RefreshConfirmCommand();
            }
        }
    }

    protected override bool CanConfirm() => !string.IsNullOrWhiteSpace(Name);
}
```

`WPF.Lib.Theme.Controls.BaseDialogView`는 이 닫기 요청을 WPF Window에 연결하는 기본 View입니다.

## 확장 지침

- UI와 무관한 상태 및 흐름만 둡니다.
- ViewModel에서 WPF 창, 메시지 상자, Dispatcher를 직접 사용하지 않습니다.
- 계산 속성은 관련 원본 속성이 바뀔 때 `OnPropertyChanged`로 알립니다.
- 공통 명령이나 검증 기능을 추가할 때 UI 독립성과 단위 테스트 가능성을 유지합니다.
