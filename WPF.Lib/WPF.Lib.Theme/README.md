# WPF.Lib.Theme

.NET 8 WPF 애플리케이션을 위한 라이트/다크 테마와 공통 컨트롤 스타일을 제공합니다. 테마 리소스를 병합하면 기본 WPF 컨트롤에 암시적 스타일이 적용되며, 실행 중에도 테마를 전환할 수 있습니다.

## 참조 및 초기 설정

저장소 루트에서 프로젝트 참조를 추가합니다.

```powershell
dotnet add <애플리케이션.csproj> reference WPF.Lib/WPF.Lib.Theme/WPF.Lib.Theme.csproj
```

`App.xaml`에서 시작 테마 하나를 병합합니다. 각 테마 사전은 `Styles/ModernControls.xaml`을 내부에서 병합하므로 스타일 사전을 따로 추가할 필요가 없습니다.

```xaml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/WPF.Lib.Theme;component/Theme/LightTheme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

다크 테마로 시작하려면 파일명을 `DarkTheme.xaml`로 바꿉니다.

## 런타임 테마 전환

`ThemeManager.ApplyTheme`은 현재 병합된 `WPF.Lib.Theme` 테마 사전을 제거하고 선택한 테마를 추가합니다.

```csharp
using WPF.Lib.Theme;

ThemeManager.ApplyTheme(ThemeKind.Dark);
ThemeManager.ApplyTheme(ThemeKind.Light);
```

실행 중 변경되어야 하는 색상 참조에는 `DynamicResource`를 사용합니다.

```xaml
<Grid Background="{DynamicResource BackgroundBrush}">
    <TextBlock Foreground="{DynamicResource ForegroundBrush}"
               Text="Theme preview" />
</Grid>
```

`ThemeManager`는 `Application.Current`가 생성된 WPF 애플리케이션에서 호출해야 합니다.

## 테마 리소스

각 색상은 `Color`와 `SolidColorBrush` 형태로 함께 제공됩니다.

| 용도 | Color 키 | Brush 키 | Light | Dark |
|---|---|---|---|---|
| 앱 배경 | `BackgroundColor` | `BackgroundBrush` | `#F7F8FA` | `#111827` |
| 컨트롤 표면 | `SurfaceColor` | `SurfaceBrush` | `#FFFFFF` | `#1F2937` |
| 기본 텍스트 | `ForegroundColor` | `ForegroundBrush` | `#1F2937` | `#F9FAFB` |
| 보조 텍스트 | `TextSecondaryColor` | `TextSecondaryBrush` | `#6B7280` | `#9CA3AF` |
| 테두리 | `BorderColor` | `BorderBrush` | `#D1D5DB` | `#4B5563` |
| 주요 동작 | `PrimaryColor` | `PrimaryBrush` | `#2563EB` | `#60A5FA` |
| 주요 동작 전경 | `PrimaryForegroundColor` | `PrimaryForegroundBrush` | `#FFFFFF` | `#FFFFFF` |
| 주요 동작 Hover | `PrimaryHoverColor` | `PrimaryHoverBrush` | `#1D4ED8` | `#93C5FD` |
| 위험 동작 | `DangerColor` | `DangerBrush` | `#DC2626` | `#F87171` |
| 교차 표면 | `SurfaceAlternateColor` | `SurfaceAlternateBrush` | `#F9FAFB` | `#182234` |
| 헤더 | `HeaderColor` | `HeaderBrush` | `#F3F4F6` | `#263244` |
| Hover | `HoverColor` | `HoverBrush` | `#EFF6FF` | `#263A55` |
| 선택 | `SelectionColor` | `SelectionBrush` | `#DBEAFE` | `#1E3A5F` |

주요 색상 위의 텍스트나 아이콘에는 고정된 흰색 대신 `PrimaryForegroundBrush`를 사용합니다.

## 제공 스타일

다음 컨트롤에는 키 없는 암시적 스타일이 자동 적용됩니다.

- `Window`, `TextBlock`, `TextBox`, `Button`
- `ComboBox`, `ComboBoxItem`
- `ScrollBar`
- `ListBox`, `ListBoxItem`
- `DataGrid`, `DataGridColumnHeader`, `DataGridCell`, `DataGridRow`
- `TabControl`, `TabItem`

필요할 때 명시적으로 사용할 수 있는 주요 스타일은 다음과 같습니다.

| 키 | 대상 | 용도 |
|---|---|---|
| `ModernWindowStyle` | `Window` | 사용자 지정 제목 표시줄과 창 제어 버튼을 포함한 창 템플릿 |
| `WindowCaptionButtonStyle` | `Button` | 최소화 및 최대화/복원 버튼 |
| `WindowCloseButtonStyle` | `Button` | 닫기 버튼 |
| `ModernListBoxItemStyle` | `ListBoxItem` | Hover 및 선택 상태가 적용된 항목 컨테이너 |
| `DataGridCellRightStyle` | `DataGridCell` | 오른쪽 정렬 셀 |
| `DataGridCellCenterStyle` | `DataGridCell` | 가운데 정렬 셀 |

## 창 제어 버튼

`WindowCaptionButton`은 가장 가까운 `Window`에 시스템 명령을 실행합니다. `Action`에는 `Minimize`, `MaximizeOrRestore`, `Close`를 지정할 수 있습니다.

```xaml
<Window xmlns:theme="clr-namespace:WPF.Lib.Theme.Controls;assembly=WPF.Lib.Theme">
    <theme:WindowCaptionButton Action="Minimize" Content="—" />
</Window>
```

일반적으로 `ModernWindowStyle`에 버튼이 포함되어 있으므로 직접 배치할 필요는 없습니다.

## 파일 구조

```text
WPF.Lib.Theme/
├─ Controls/
│  └─ WindowCaptionButton.cs
├─ Styles/
│  ├─ Controls/
│  │  ├─ Button.xaml
│  │  ├─ ComboBox.xaml
│  │  ├─ DataGrid.xaml
│  │  ├─ ListBox.xaml
│  │  ├─ ScrollBar.xaml
│  │  ├─ TabControl.xaml
│  │  ├─ TextBlock.xaml
│  │  ├─ TextBox.xaml
│  │  └─ Window.xaml
│  └─ ModernControls.xaml
├─ Theme/
│  ├─ DarkTheme.xaml
│  └─ LightTheme.xaml
├─ ThemeManager.cs
└─ WPF.Lib.Theme.csproj
```

## 확장 지침

1. 새 색상이나 브러시는 `LightTheme.xaml`과 `DarkTheme.xaml`에 동일한 키로 추가합니다.
2. 컨트롤별 스타일은 `Styles/Controls/`에 두고 `ModernControls.xaml`에서 병합합니다.
3. 테마에 따라 바뀌는 값에는 하드코딩된 색상 대신 `DynamicResource`를 사용합니다.
4. 기본, Hover, Focus, 선택, 비활성 상태를 함께 확인합니다.
5. 주요 색상 배경 위의 전경에는 `PrimaryForegroundBrush`를 사용합니다.
