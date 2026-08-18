# Drag and Drop

`WPF.Lib.Controls.DragDrop`은 WPF 이벤트를 뷰 모델의 `ICommand`로 연결하는 재사용 가능한 드래그 앤 드롭 기반 구조입니다. 외부 파일 드롭과 `ListBox`, `TreeView`, `DataGrid` 등 `ItemsControl` 항목 드래그를 동일한 방식으로 처리합니다.

## 구성 요소

| API | 역할 |
|---|---|
| `DragDropBehavior.IsDragSource` | `ItemsControl`의 항목 드래그 시작 활성화 |
| `DragDropBehavior.IsDropTarget` | 요소의 드롭 수신 활성화 |
| `DragDropBehavior.DropCommand` | 드래그 검증과 드롭 처리를 담당하는 `ICommand` |
| `DragDropBehavior.DragGroup` | 서로 호환되는 내부 드래그 영역 구분 |
| `DragDropBehavior.DropEffect` | 허용 시 표시할 `Move` 또는 `Copy` 효과 |
| `DragDropBehavior.IsDragOver` | 현재 유효한 데이터가 위에 있는지 나타내는 읽기 전용 상태 |
| `DragDropInfo` | 드롭 명령에 전달되는 컨텍스트 |

`DropCommand.CanExecute`가 `true`를 반환해야 드롭이 허용됩니다. 실제 드롭 시 같은 `DragDropInfo`로 `Execute`가 호출되므로, 허용 조건과 처리 로직을 뷰 모델에 둘 수 있습니다.

## 네임스페이스

```xaml
xmlns:dragDrop="clr-namespace:WPF.Lib.Controls.DragDrop;assembly=WPF.Lib.Controls"
```

뷰 모델에서는 다음 네임스페이스를 사용합니다.

```csharp
using WPF.Lib.Controls.DragDrop;
```

## ListBox 항목 재정렬

드래그 원본과 드롭 대상을 함께 활성화하고 같은 그룹명을 지정합니다.

```xaml
<ListBox ItemsSource="{Binding Items}"
         dragDrop:DragDropBehavior.IsDragSource="True"
         dragDrop:DragDropBehavior.IsDropTarget="True"
         dragDrop:DragDropBehavior.DragGroup="EditorItems"
         dragDrop:DragDropBehavior.DropCommand="{Binding ReorderCommand}" />
```

목록은 `ObservableCollection<T>`처럼 이동 결과를 알릴 수 있는 컬렉션을 권장합니다.

```csharp
public ObservableCollection<EditorItem> Items { get; } = [];

public RelayCommand ReorderCommand { get; }

private bool CanReorder(object? parameter)
{
    return parameter is DragDropInfo { DraggedItem: EditorItem item, InsertionIndex: >= 0 } &&
           Items.Contains(item);
}

private void Reorder(object? parameter)
{
    if (parameter is not DragDropInfo { DraggedItem: EditorItem item } info)
    {
        return;
    }

    var oldIndex = Items.IndexOf(item);
    var newIndex = Math.Clamp(info.InsertionIndex, 0, Items.Count);
    if (oldIndex < newIndex)
    {
        newIndex--;
    }

    if (oldIndex >= 0 && oldIndex != newIndex)
    {
        Items.Move(oldIndex, newIndex);
    }
}
```

`InsertionIndex`는 드롭 시점의 컬렉션 삽입 위치입니다. 같은 목록에서 아래 방향으로 이동할 때는 기존 항목이 제거되며 인덱스가 하나 줄어드므로 예제처럼 보정합니다. `TargetItem`과 `Position`이 필요하면 항목 기준의 별도 정책도 구현할 수 있습니다.

## 외부 파일 드롭

파일 드롭 영역은 드롭 대상과 명령만 지정합니다. 파일 복사 의미를 나타내려면 `DropEffect="Copy"`를 사용합니다.

```xaml
<Border dragDrop:DragDropBehavior.IsDropTarget="True"
        dragDrop:DragDropBehavior.DropEffect="Copy"
        dragDrop:DragDropBehavior.DropCommand="{Binding DropFilesCommand}">
    <TextBlock Text="파일을 놓으세요" />
</Border>
```

```csharp
DropFilesCommand = new RelayCommand(
    parameter =>
    {
        if (parameter is DragDropInfo info)
        {
            ImportFiles(info.Files);
        }
    },
    parameter => parameter is DragDropInfo info &&
                 info.Files.Any(IsSupportedFile));
```

파일 경로는 `DragDropInfo.Files`에 담깁니다. 확장자, 파일 존재 여부, 최대 개수 같은 정책은 `CanExecute`에서 검증하고, 읽기 실패에 대한 예외 처리는 `Execute`에서 수행합니다.

## 드롭 상태 표시

`IsDragOver`는 `DropCommand.CanExecute`가 `true`인 동안에만 활성화됩니다. 스타일 트리거나 오버레이의 `Visibility`에 사용할 수 있습니다.

```xaml
<Style TargetType="ListBox" BasedOn="{StaticResource {x:Type ListBox}}">
    <Style.Triggers>
        <Trigger Property="dragDrop:DragDropBehavior.IsDragOver" Value="True">
            <Setter Property="BorderBrush" Value="{DynamicResource PrimaryBrush}" />
            <Setter Property="BorderThickness" Value="2" />
        </Trigger>
    </Style.Triggers>
</Style>
```

## DragDropInfo

| 속성 | 설명 |
|---|---|
| `Source`, `Target` | 드래그를 시작한 요소와 드롭 대상 요소 |
| `DraggedItem` | 내부 `ItemsControl`에서 드래그한 항목. 외부 파일이면 `null` |
| `TargetItem` | 포인터 아래에 있는 대상 항목 |
| `InsertionIndex` | 대상 컬렉션에 삽입할 인덱스. 일반 요소에서는 `-1` |
| `Position` | 대상 항목의 `Before`, `After`, 또는 `None` |
| `Files` | Windows 파일 드롭 경로 목록 |
| `Data` | 추가 포맷을 읽을 수 있는 원본 `IDataObject` |

## 확장 지침

- 서로 다른 목록 사이에서 이동할 때는 양쪽에 같은 `DragGroup`을 지정하고, 명령에서 원본·대상 컬렉션을 갱신합니다.
- `TreeView` 계층 이동은 `TargetItem`을 부모 후보로 사용하고 순환 참조를 `CanExecute`에서 차단합니다.
- `DataGrid` 행 재정렬도 같은 방식으로 사용할 수 있습니다. 셀 편집과 충돌한다면 드래그 핸들 요소에 `IsDragSource`를 적용하는 전용 템플릿을 구성합니다.
- 비밀번호나 임시 파일처럼 민감한 데이터는 `DragDropInfo.Data`에 그대로 보관하거나 로그로 남기지 않습니다.
