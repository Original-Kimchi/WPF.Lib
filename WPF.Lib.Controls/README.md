# WPF.Lib.Controls

.NET 8 WPF 애플리케이션에서 재사용할 수 있는 입력, 피드백, 이미지 표시 컨트롤과 드래그 앤 드롭 동작을 제공합니다. 모든 주요 상태는 의존성 속성 또는 바인딩 가능한 모델로 노출되며, `ImageModel`과 `ImageAnnotation`은 `WPF.Lib.MVVM`의 변경 알림 기반 클래스를 사용합니다.

## 제공 기능

| 컨트롤 | 용도 | 주요 API |
|---|---|---|
| `NumericUpDown` | 범위와 증감 단위를 지원하는 숫자 입력 | `Value`, `Minimum`, `Maximum`, `Increment`, `DecimalPlaces`, `IsReadOnly` |
| `TimePicker` | 12시간제 또는 24시간제 시간 선택 | `SelectedTime`, `MinuteStep`, `Is24HourMode` |
| `SearchBox` | Enter 검색과 입력 지연 검색 | `Text`, `Placeholder`, `SearchCommand`, `SearchCommandParameter`, `SearchOnTextChanged`, `SearchDelay` |
| `BusyOverlay` | 콘텐츠 위에 진행 상태 표시 | `Child`, `ChildTemplate`, `IsBusy`, `BusyMessage`, `IsIndeterminate`, `Progress` |
| `ToastHost` | 자동 닫힘을 지원하는 알림 목록 표시 | `Service` |
| `ThemedMessageBox` | 현재 테마를 따르는 모달 메시지 박스 | `Show(...)`, `MessageBoxButton`, `MessageBoxImage`, `MessageBoxResult` |
| `ImageViewer` | 확대, 이동, 미니맵, 선·사각형·타원 주석 | `Model`, `ShowMiniMap`, `FitToViewport()` |
| `ImageMiniMap` | 독립 배치 가능한 이미지 미니맵 | `Model` |
| `DragDropBehavior` | 파일 드롭과 `ItemsControl` 항목 이동 | `IsDragSource`, `IsDropTarget`, `DropCommand`, `DragGroup`, `IsDragOver` |
| `NestedScrollBehavior` | 안쪽 스크롤 경계에서 휠 입력을 바깥 스크롤로 전달 | `IsEnabled` |

## 참조 및 테마 설정

저장소 루트에서 애플리케이션 프로젝트에 참조를 추가합니다.

```powershell
dotnet add <애플리케이션.csproj> reference WPF.Lib/WPF.Lib.Controls/WPF.Lib.Controls.csproj
```

`WPF.Lib.Controls.Controls`는 `AssemblyInfo.cs`의 `XmlnsDefinition`으로 기본 WPF XAML 네임스페이스에 연결되어 있습니다. 라이브러리를 참조하는 앱에서는 접두사 없이 컨트롤을 사용할 수 있습니다.

```xaml
<Grid xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <ImageViewer Model="{Binding Image}" ShowMiniMap="False" />
    <ImageMiniMap Model="{Binding Image}" Width="190" Height="130"
                  HorizontalAlignment="Right" VerticalAlignment="Bottom" />
</Grid>
```

이 매핑은 `Controls` 네임스페이스에만 적용됩니다. `Models`, `Converters`, `DragDrop`, `Behaviors` 등은 기존처럼 별도 네임스페이스를 선언합니다. 기존 `controls:` 표기도 계속 사용할 수 있으며, 아래 예제는 명시적인 표기를 유지합니다.

```xaml
xmlns:controls="clr-namespace:WPF.Lib.Controls.Controls;assembly=WPF.Lib.Controls"
```

`AssemblyInfo.cs`는 어셈블리 특성을 모아 두는 일반 C# 파일입니다. SDK의 자동 생성 설정을 끌 필요 없이 이 파일에 사용자 정의 특성을 추가할 수 있습니다. 동일한 XAML 네임스페이스에 같은 이름의 타입을 노출하면 충돌할 수 있으므로 컨트롤 이름은 고유하게 유지합니다.

컨트롤 템플릿은 `WPF.Lib.Theme`의 동적 브러시 리소스를 사용합니다. 애플리케이션에서 `WPF.Lib.Theme`을 참조하고 `App.xaml`에 라이트 또는 다크 테마를 병합합니다.

```xaml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/WPF.Lib.Theme;component/Theme/LightTheme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

테마 추가 방법과 런타임 전환은 [`WPF.Lib.Theme` 문서](../WPF.Lib.Theme/README.md)를 참고합니다.

## 기본 컨트롤 사용법

`Value`, `SelectedTime`, `Text`는 기본적으로 양방향 바인딩됩니다.

```xaml
<StackPanel>
    <controls:NumericUpDown Width="180"
                            Value="{Binding Quantity}"
                            Minimum="0"
                            Maximum="100"
                            Increment="0.5"
                            DecimalPlaces="1" />

    <controls:TimePicker Margin="0,12,0,0"
                         SelectedTime="{Binding StartTime}"
                         MinuteStep="10"
                         Is24HourMode="True" />

    <controls:SearchBox Margin="0,12,0,0"
                        Text="{Binding SearchText}"
                        Placeholder="컨트롤 검색"
                        SearchCommand="{Binding SearchCommand}"
                        SearchOnTextChanged="True"
                        SearchDelay="350" />
</StackPanel>
```

`SearchCommandParameter`를 지정하지 않으면 현재 `Text`가 명령 인수로 전달됩니다. `SearchOnTextChanged`가 `False`여도 Enter를 누르면 검색 명령이 실행됩니다.

## BusyOverlay

`IsIndeterminate`가 `False`이면 `Progress`에 0부터 100 사이의 진행률을 바인딩합니다. 범위를 벗어난 값은 자동으로 보정됩니다.

```xaml
<controls:BusyOverlay IsBusy="{Binding IsBusy}"
                      BusyMessage="데이터를 불러오는 중..."
                      IsIndeterminate="False"
                      Progress="{Binding Progress}">
    <controls:BusyOverlay.Child>
        <DataGrid ItemsSource="{Binding Items}" />
    </controls:BusyOverlay.Child>
</controls:BusyOverlay>
```

## 토스트 알림

`ToastService` 인스턴스를 화면과 뷰 모델이 공유하도록 등록합니다. Microsoft.Extensions.DependencyInjection을 사용한다면 다음과 같이 싱글턴으로 등록할 수 있습니다.

```csharp
using WPF.Lib.Controls.Abstractions;
using WPF.Lib.Controls.Services;

services.AddSingleton<IToastService, ToastService>();
```

호스트를 최상위 화면에 배치하고 동일한 서비스를 바인딩합니다.

```xaml
<controls:ToastHost HorizontalAlignment="Right"
                    VerticalAlignment="Top"
                    Service="{Binding ToastService}" />
```

뷰 모델이나 서비스에서 알림을 표시합니다.

```csharp
using WPF.Lib.Controls.Abstractions;
using WPF.Lib.Controls.Models;

public sealed class EditorViewModel(IToastService toastService)
{
    public void NotifySaved()
    {
        toastService.Show(
            "변경 사항을 저장했습니다.",
            ToastType.Success,
            duration: TimeSpan.FromSeconds(3));
    }
}
```

`MaximumVisible`의 기본값은 4개입니다. `duration`을 `TimeSpan.Zero` 이하로 지정하면 자동으로 닫히지 않으며, `Dismiss` 또는 `Clear`로 제거할 수 있습니다.

`ToastService`는 생성된 UI Dispatcher를 소유권 기준으로 사용합니다. 백그라운드 호출은 해당 Dispatcher로 전달하며, `Dismiss`, 최대 개수 초과, `Clear`, `Dispose`에서 연관된 타이머를 중지하고 `Tick` 구독을 해제합니다. DI Singleton으로 등록하면 루트 `ServiceProvider.Dispose()`가 마지막 정리를 호출합니다.

## 테마 메시지 박스

`ThemedMessageBox`는 WPF 기본 메시지 박스와 동일한 버튼, 아이콘, 결과 열거형을 사용합니다. 활성 창을 자동으로 소유자로 지정하거나 첫 번째 인수로 명시할 수 있습니다.

```csharp
using System.Windows;
using WPF.Lib.Controls.Controls;

var result = ThemedMessageBox.Show(
    owner: Application.Current.MainWindow,
    messageBoxText: "변경 내용을 저장하시겠습니까?",
    caption: "저장 확인",
    button: MessageBoxButton.YesNoCancel,
    icon: MessageBoxImage.Question,
    defaultResult: MessageBoxResult.Yes);

if (result == MessageBoxResult.Yes)
{
    Save();
}
```

`OK`, `OKCancel`, `YesNo`, `YesNoCancel` 버튼 조합과 정보, 경고, 오류, 질문 아이콘을 지원합니다. `MessageBoxType`에는 `Default`, `Information`, `Success`, `Warning`, `Error`가 있으며 상태에 맞는 강조선과 아이콘 색상을 표시합니다. `Ctrl+C`를 누르면 제목, 메시지, 버튼 텍스트가 클립보드에 복사됩니다.

## ImageViewer

이미지와 뷰어 상태를 `ImageModel`에 설정한 뒤 `Model`에 바인딩합니다.

```csharp
using System.Windows.Media.Imaging;
using WPF.Lib.Controls.Models;

public ImageModel Image { get; } = new()
{
    Source = new BitmapImage(new Uri("pack://application:,,,/Assets/sample.png")),
    UnitsPerPixel = 0.25,
    MeasurementUnit = "mm"
};
```

```xaml
<controls:ImageViewer Model="{Binding Image}" />
```

- `Ctrl`+마우스 휠로 확대·축소하며 배율은 0.01에서 10 사이로 제한됩니다.
- `Pan` 모드에서는 이미지를 이동하고 기존 주석을 선택, 이동, 크기 조절할 수 있습니다. 사각형과 타원은 네 모서리 및 상·하·좌·우 중앙 핸들로 크기를 조절합니다.
- 선택한 주석은 방향키로 1px, `Shift`+방향키로 10px씩 이동할 수 있습니다.
- 주석을 이동하거나 크기를 조절할 때 `Alt`를 누르면 ImageViewer 영역과 다른 주석의 좌·중앙·우 및 상·중앙·하 위치에 스냅되며 정렬 가이드가 표시됩니다.
- `Line`, `Rectangle`, `Ellipse` 모드에서는 드래그하여 주석을 추가합니다. 추가 후에는 자동으로 `Pan` 모드로 돌아갑니다.
- `SetDrawingModeCommand`, `ResetScaleCommand`, `DeleteSelectedCommand`, `ClearAnnotationsCommand`로 도구 모음을 구성할 수 있습니다.
- `UnitsPerPixel`과 `MeasurementUnit`으로 주석 측정값의 단위를 설정할 수 있습니다.
- `ImageAnnotation.ObjectColor`로 각 주석의 선과 채우기 색상을 개별 설정할 수 있습니다.
- `ImageModel.ShowLabels`로 ImageViewer 전체 Label 표시 여부를 설정할 수 있으며 기본값은 `false`입니다.
- `ImageAnnotation.LabelVisibility`는 `WhenSelected`, `Always`, `Hidden` 중 하나를 사용합니다.
  새 주석의 기본값은 `Always`입니다.

주석에 별도 데이터나 동작이 필요하면 `ImageModel.CreateAnnotation`을 재정의하여 사용자 정의 `ImageAnnotation`을 반환할 수 있습니다.

## 수명과 메모리

- `SearchBox`는 `Unloaded`에서 입력 지연용 `DispatcherTimer`를 중지합니다. 다시 로드된 뒤 입력이 들어오면 같은 타이머를 재사용합니다.
- `ImageViewer`는 지연된 맞춤 작업을 하나만 유지하고 `Unloaded`에서 예약된 `DispatcherOperation`과 마우스 캡처를 취소합니다.
- 대용량 이미지는 원본 전체 해상도로 디코딩하지 말고 표시 크기에 맞는 `DecodePixelWidth`/`DecodePixelHeight`와 최대 픽셀 수 제한을 사용합니다.
- 이미지 스트림을 사용할 때는 디코딩이 끝난 뒤 스트림을 닫을 수 있도록 `BitmapCacheOption.OnLoad`를 사용하고, 화면 이탈 시 ViewModel의 이미지 참조를 제거합니다.
- `Loaded`에서 등록한 인스턴스 이벤트는 `Unloaded`에서 해제합니다. 정적 이벤트는 약한 이벤트 패턴 또는 명시적 `Dispose`가 필요합니다.

상세한 소유권 기준과 현재 보강 항목은 [라이브러리 수명·메모리·안전 가이드](../../docs/library-lifecycle-and-safety.md)를 참고합니다.

## 독립 미니맵

`ImageMiniMap`은 Viewer 참조 없이 동일한 `ImageModel` 인스턴스를 바인딩하여 사용합니다.
원하는 패널에 배치하고 `Width`/`Height`로 크기를 지정하거나 부모 레이아웃에 맞게 늘릴 수 있습니다.

```xaml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="220" />
    </Grid.ColumnDefinitions>
    <controls:ImageViewer Model="{Binding Image}" ShowMiniMap="False" />
    <controls:ImageMiniMap Grid.Column="1" Model="{Binding Image}"
                           Height="150" Margin="12" VerticalAlignment="Top" />
</Grid>
```

- `ShowMiniMap` 기본값은 `true`이며 기존 내장 미니맵도 `ImageMiniMap`을 사용합니다.
- Viewer의 확대·스크롤·크기 변경은 모델의 읽기 전용 `Viewport`에 이미지 좌표로 반영됩니다.
- 미니맵을 클릭하거나 드래그하면 해당 이미지 위치를 중심으로 Viewer가 이동합니다. 이미지 비율에 따른 여백은 클릭 대상에서 제외됩니다.
- 코드에서는 `ImageModel.NavigateTo(new Point(x, y))`로 이동할 수 있습니다.
- Model 하나에 Viewer 하나와 여러 미니맵을 연결할 수 있습니다. 서로 다른 표시 영역을 가진 Viewer 여러 개는 별도의 Model을 사용합니다.
- Viewer 없이도 이미지를 표시할 수 있으며, 연결된 Viewer가 표시 영역을 제공하기 전에는 영역 사각형이 숨겨집니다.

## Drag and Drop

`DragDropBehavior` 연결 속성으로 `ListBox`, `TreeView`, `DataGrid` 등의 항목 드래그와 외부 파일 드롭을 뷰 모델 명령에 연결할 수 있습니다. 드롭 명령은 `DragDropInfo`를 받아 원본 항목, 대상 항목, 삽입 위치, 파일 경로를 확인합니다.

```xaml
<ListBox ItemsSource="{Binding Items}"
         dragDrop:DragDropBehavior.IsDragSource="True"
         dragDrop:DragDropBehavior.IsDropTarget="True"
         dragDrop:DragDropBehavior.DragGroup="Items"
         dragDrop:DragDropBehavior.DropCommand="{Binding ReorderCommand}" />
```

공개 API, 목록 재정렬, 파일 드롭, 상태 표시 방법은 [Drag and Drop 문서](DRAG_DROP.md)를 참고합니다.

## 중첩 스크롤

`NestedScrollBehavior`는 라이브러리가 로드될 때 처리기가 등록되지만 기본값은 비활성화되어 있습니다. 중첩 스크롤이 필요한 상위 영역에서 명시적으로 활성화하면, 안쪽 `ScrollViewer`가 위·아래 경계에 도달했을 때 휠 입력을 스크롤 가능한 부모로 전달합니다. Shift+휠은 가로 스크롤에 사용합니다. 연결 속성 값은 하위 요소로 상속됩니다.

```xaml
<ScrollViewer xmlns:behaviors="clr-namespace:WPF.Lib.Controls.Behaviors;assembly=WPF.Lib.Controls"
              behaviors:NestedScrollBehavior.IsEnabled="True">
    <!-- 이 영역의 중첩 ScrollViewer는 경계에서 휠 입력을 부모로 전달합니다. -->
</ScrollViewer>
```

## 파일 구조

```text
WPF.Lib.Controls/
├─ Abstractions/
│  └─ IToastService.cs
├─ Behaviors/
│  └─ NestedScrollBehavior.cs
├─ Controls/
│  ├─ BusyOverlay.xaml
│  ├─ ImageMiniMap.xaml
│  ├─ ImageViewer.xaml
│  ├─ NumericUpDown.xaml
│  ├─ SearchBox.xaml
│  ├─ ThemedMessageBox.xaml
│  ├─ TimePicker.xaml
│  └─ ToastHost.xaml
├─ Converters/
├─ DragDrop/
│  ├─ DragDropBehavior.cs
│  ├─ DragDropInfo.cs
│  └─ DropPosition.cs
├─ Models/
├─ Services/
│  └─ ToastService.cs
├─ ControlsModuleInitializer.cs
└─ WPF.Lib.Controls.csproj
```

## 빌드 및 검증

저장소 루트에서 실행합니다.

```powershell
dotnet build WPFControls.UI.sln --configuration Debug
dotnet format WPFControls.UI.sln --verify-no-changes
```

컨트롤을 변경할 때는 라이트·다크 테마에서 기본, hover, focus, 선택, 비활성, 팝업 상태를 함께 확인합니다. 템플릿의 텍스트와 아이콘에는 시스템 기본색 대신 적절한 동적 테마 브러시를 사용합니다.

새 솔루션의 프로젝트 배치와 앱 초기화 전체 예시는 [WPF.Lib 도입 가이드](../../docs/library-adoption-guide.md)를 참고합니다.
