# WPFControls.Theme 사용 가이드

`WPFControls.Theme`는 .NET 8 WPF 애플리케이션에서 사용할 수 있는 라이트/다크 테마와 공통 컨트롤 스타일을 제공합니다.

## 프로젝트 참조

테마를 사용할 WPF 프로젝트에 참조를 추가합니다.

```powershell
dotnet add WPFControls.UI/WPFControls.UI.csproj reference WPFControls.Theme/WPFControls.Theme.csproj
```

## 테마 적용

`App.xaml`의 병합 사전에 라이트 또는 다크 테마 하나를 등록합니다. 각 테마 파일은 공통 스타일 모음인 `Styles/ModernControls.xaml`을 자동으로 병합합니다.

```xaml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/WPFControls.Theme;component/Theme/LightTheme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

다크 테마를 기본값으로 사용하려면 파일명을 `DarkTheme.xaml`로 변경합니다.

## 런타임 테마 전환

`ThemeManager.ApplyTheme`은 현재 등록된 테마 사전을 찾아 교체합니다.

```csharp
using WPFControls.Theme;

ThemeManager.ApplyTheme(ThemeKind.Dark);
ThemeManager.ApplyTheme(ThemeKind.Light);
```

런타임 전환이 필요한 색상 참조에는 반드시 `DynamicResource`를 사용합니다.

```xaml
<Grid Background="{DynamicResource BackgroundBrush}">
    <TextBlock Foreground="{DynamicResource ForegroundBrush}"
               Text="Theme preview" />
</Grid>
```

## 색상 리소스

색상 값에는 `Color`, UI 속성에는 대응하는 `Brush` 키를 사용합니다.

| 용도 | Color 키 | Brush 키 | Light | Dark |
|---|---|---|---|---|
| 앱 배경 | `BackgroundColor` | `BackgroundBrush` | `#F7F8FA` | `#111827` |
| 컨트롤 표면 | `SurfaceColor` | `SurfaceBrush` | `#FFFFFF` | `#1F2937` |
| 기본 글자 | `ForegroundColor` | `ForegroundBrush` | `#1F2937` | `#F9FAFB` |
| 보조 글자 | `TextSecondaryColor` | `TextSecondaryBrush` | `#6B7280` | `#9CA3AF` |
| 테두리 | `BorderColor` | `BorderBrush` | `#D1D5DB` | `#4B5563` |
| Primary | `PrimaryColor` | `PrimaryBrush` | `#2563EB` | `#60A5FA` |
| Primary 전경 | `PrimaryForegroundColor` | `PrimaryForegroundBrush` | `#FFFFFF` | `#FFFFFF` |
| Primary Hover | `PrimaryHoverColor` | `PrimaryHoverBrush` | `#1D4ED8` | `#93C5FD` |
| 위험 상태 | `DangerColor` | `DangerBrush` | `#DC2626` | `#F87171` |
| 교차 표면 | `SurfaceAlternateColor` | `SurfaceAlternateBrush` | `#F9FAFB` | `#182234` |
| 헤더 | `HeaderColor` | `HeaderBrush` | `#F3F4F6` | `#263244` |
| Hover | `HoverColor` | `HoverBrush` | `#EFF6FF` | `#263A55` |
| 선택 | `SelectionColor` | `SelectionBrush` | `#DBEAFE` | `#1E3A5F` |

기본 글자색은 `ForegroundBrush`입니다. 보조 설명에는 `TextSecondaryBrush`를 사용합니다.

## Primary 배경과 전경

`PrimaryBrush`를 배경으로 사용하는 텍스트와 아이콘에는 반드시 `PrimaryForegroundBrush`를 사용합니다. `White`, `Black` 같은 고정 색상을 직접 지정하지 않습니다.

```xaml
<Border Background="{DynamicResource PrimaryBrush}">
    <TextBlock Foreground="{DynamicResource PrimaryForegroundBrush}"
               Text="Primary content" />
</Border>
```

기본 `Button` 스타일은 이 규칙을 자동으로 적용합니다. Button 템플릿 내부의 문자열과 `TextBlock` 콘텐츠에도 Button의 `Foreground`가 전달됩니다.

## 제공 컨트롤 스타일

테마를 병합하면 아래 암시적 스타일이 자동 적용됩니다.

| 컨트롤 | 주요 동작 |
|---|---|
| `Window` | 사용자 지정 제목 표시줄, 최소화/최대화/닫기 버튼, 테마 배경과 전경 |
| `TextBlock` | `ForegroundBrush` 기본 적용 |
| `TextBox` | 테마 표면과 테두리, Hover/Focus/Disabled 상태 |
| `Button` | Primary 배경과 전경, Hover/Pressed/Disabled 상태 |
| `ComboBox` | 전체 영역 클릭, 테마 드롭다운과 화살표, Focus/Open/Disabled 상태 |
| `ComboBoxItem` | Hover/Selected/Disabled 상태 |
| `ListBox`, `ListBoxItem` | 테마 테두리, Hover/Selected 상태 |
| `DataGrid` | 헤더, 행, 셀, 교차 행, 전체 행 선택 |
| `TabControl`, `TabItem` | 탭 배경, Hover/Selected/Disabled 상태 |

다음 명명된 스타일도 제공합니다.

- `ModernWindowStyle`: 사용자 지정 Window 템플릿
- `WindowCaptionButtonStyle`: 제목 표시줄 버튼
- `WindowCloseButtonStyle`: 닫기 버튼
- `DataGridCellRightStyle`: 오른쪽 정렬 DataGrid 셀
- `DataGridCellCenterStyle`: 가운데 정렬 DataGrid 셀

## 선택, 포커스 및 Hover 규칙

컨트롤 상태에 Windows 기본 시스템 색상이나 기본 컨트롤 크롬이 노출되지 않도록 합니다.

- Hover 상태는 `HoverBrush` 또는 `PrimaryHoverBrush`를 사용합니다.
- 선택 상태는 `SelectionBrush`를 사용합니다.
- 포커스 상태는 `PrimaryBrush`를 중심으로 표현합니다.
- 일반 테두리는 `BorderBrush`를 사용합니다.
- 템플릿 내부의 `ToggleButton` 같은 보조 컨트롤도 시스템 기본 템플릿에 의존하지 않습니다.

현재 ComboBox는 내부의 투명한 ToggleButton이 전체 너비를 덮습니다. 따라서 선택된 항목 영역과 화살표 영역 모두 클릭할 수 있으며, 열림 상태에서도 Windows 기본 선택 색상이 표시되지 않습니다. 편집 가능한 ComboBox의 텍스트 입력 영역은 그대로 유지됩니다.

## 테마 확장 규칙

공통 스타일은 `Styles/ModernControls.xaml`에서 관리하며 컨트롤별 XAML은 `Styles/Controls/`에 둡니다.

새 리소스나 스타일을 추가할 때는 다음 규칙을 지킵니다.

1. `LightTheme.xaml`과 `DarkTheme.xaml`에 동일한 리소스 키를 추가합니다.
2. 컨트롤 스타일에서는 하드코딩한 색상 대신 의미 기반 `DynamicResource`를 사용합니다.
3. 선택, Hover, 포커스, 비활성 상태를 모두 명시합니다.
4. Windows 기본 시스템 색상이나 기본 크롬이 드러나지 않는지 확인합니다.
5. Primary 배경 위 텍스트와 아이콘에는 `PrimaryForegroundBrush`를 사용합니다.
6. 새 컨트롤 사전은 `Styles/ModernControls.xaml`에 병합합니다.

## 현재 파일 구조

```text
WPFControls.Theme/
├─ Theme/
│  ├─ LightTheme.xaml
│  └─ DarkTheme.xaml
├─ Styles/
│  ├─ ModernControls.xaml
│  └─ Controls/
│     ├─ Window.xaml
│     ├─ TextBlock.xaml
│     ├─ TextBox.xaml
│     ├─ Button.xaml
│     ├─ ComboBox.xaml
│     ├─ ListBox.xaml
│     ├─ DataGrid.xaml
│     └─ TabControl.xaml
├─ Controls/
│  └─ WindowCaptionButton.cs
└─ ThemeManager.cs
```
