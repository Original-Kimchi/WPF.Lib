# WPFControls.Theme 사용법

## 프로젝트 참조

테마를 사용할 WPF 프로젝트에 참조를 추가합니다.

```powershell
dotnet add WPFControls.UI/WPFControls.UI.csproj reference WPFControls.Theme/WPFControls.Theme.csproj
```

## 기본 테마 적용

`App.xaml`의 병합 사전에 원하는 테마 하나를 등록합니다.

```xaml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/WPFControls.Theme;component/Theme/LightTheme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

다크 테마는 파일명을 `DarkTheme.xaml`로 변경합니다. 컨트롤에서는 의미 기반 리소스 키를 사용합니다.

```xaml
<Grid Background="{DynamicResource BackgroundBrush}">
    <TextBlock Foreground="{DynamicResource TextPrimaryBrush}"
               Text="Theme preview" />
</Grid>
```

런타임 전환을 지원하려면 `DynamicResource`를 사용합니다.

## 런타임 테마 전환

`ThemeManager`가 기존 테마 사전을 찾아 교체합니다.

```csharp
using WPFControls.Theme;

ThemeManager.ApplyTheme(ThemeKind.Dark);
ThemeManager.ApplyTheme(ThemeKind.Light);
```

## 제공 스타일

테마를 병합하면 다음 컨트롤의 암시적 스타일이 자동 적용됩니다.

- `Window`, `TextBlock`, `TextBox`, `Button`, `ComboBox`
- `ListBox`, `ListBoxItem`
- `DataGrid`, 행, 셀, 열 머리글
- `TabControl`, `TabItem`

개별 컨트롤에 별도 `Style`을 지정할 필요가 없습니다. 배경과 전경을 직접 지정할 때는 `BackgroundBrush`, `SurfaceBrush`, `TextPrimaryBrush`, `TextSecondaryBrush`, `PrimaryBrush` 등의 `DynamicResource`를 사용합니다.

공통 컨트롤 스타일은 `Styles/ModernControls.xaml`에서 관리합니다. 두 테마는 같은 색상 및 브러시 키를 유지해야 하므로 새 키를 추가할 때는 `LightTheme.xaml`과 `DarkTheme.xaml`을 함께 수정합니다.
