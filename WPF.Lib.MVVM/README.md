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
| `AsyncRelayCommand` | 비동기 `ICommand`, 재진입 방지, 실행 상태 및 취소 제공 |
| `INavigationAware` | 화면 진입·이탈 시 리소스를 활성화하거나 정리하는 수명 계약 |
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

`RelayCommand`는 동기 작업에 사용합니다. I/O처럼 비동기로 실행되는 작업에는 `AsyncRelayCommand`를 사용합니다. 실행 중에는 자동으로 `CanExecute`가 `false`가 되며, `IsRunning`을 로딩 UI에 바인딩할 수 있습니다. 취소 토큰을 받는 대리자는 `Cancel()`로 취소할 수 있고, 직접 실행 결과를 기다려야 할 때는 `ExecuteAsync()`를 사용합니다.

```csharp
public AsyncRelayCommand LoadCommand { get; }

private readonly ICustomerService _customerService;
private readonly IExceptionHandler _exceptionHandler;

public CustomerViewModel(
    ICustomerService customerService,
    IExceptionHandler exceptionHandler)
{
    _customerService = customerService;
    _exceptionHandler = exceptionHandler;
    LoadCommand = new AsyncRelayCommand(
        (_, cancellationToken) => LoadAsync(cancellationToken));
}

private async Task LoadAsync(CancellationToken cancellationToken)
{
    try
    {
        await _customerService.LoadAsync(cancellationToken);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        // 사용자가 취소한 작업입니다.
    }
    catch (Exception exception)
    {
        _exceptionHandler.Handle(exception, nameof(CustomerViewModel));
    }
}
```

WPF가 `ICommand.Execute`를 호출하는 경로는 반환값이 없는 `async void` 경계입니다. 실행 대리자에서 빠져나온 예외는 호출자가 기다릴 수 없으므로 예상 가능한 파일·네트워크 예외를 대리자 내부에서 처리합니다. 테스트나 코드에서 완료·예외를 직접 관찰해야 할 때는 `ExecuteAsync()`를 호출해 기다립니다.

## 셸 ViewModel

`BaseMainViewModel<TMenu>`는 `SelectedMenu`가 바뀌면 `ResolveViewModel`을 호출해 `CurrentViewModel`을 교체합니다. 이전·다음 ViewModel이 `INavigationAware`를 구현하면 각각 `OnDeactivated`, `OnActivated`를 자동으로 호출합니다. 파생 클래스 생성이 끝난 뒤 `SelectInitialMenu`를 호출해야 첫 메뉴가 선택됩니다.

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

Singleton 셸이 화면 ViewModel을 계속 보유하면 화면을 전환해도 해당 ViewModel 인스턴스는 유지됩니다. 리소스가 큰 화면은 Transient/Scope로 만들거나 `INavigationAware`를 구현해 `OnDeactivated`에서 취소·구독 해제·이미지 참조 제거를 수행합니다.

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

수명 선택과 해제 기준은 [라이브러리 수명·메모리·안전 가이드](../../docs/library-lifecycle-and-safety.md)를 참고합니다.
