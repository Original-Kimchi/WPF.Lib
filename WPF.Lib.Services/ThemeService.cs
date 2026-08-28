using WPF.Lib.Core.Abstractions;
using WPF.Lib.Core.Models;
using WPF.Lib.Theme;

namespace WPF.Lib.Services;

public sealed class ThemeService : IThemeService
{
    public ApplicationTheme CurrentTheme { get; private set; } = ApplicationTheme.Light;

    public void ApplyTheme(ApplicationTheme theme)
    {
        ThemeManager.ApplyTheme(theme == ApplicationTheme.Light ? ThemeKind.Light : ThemeKind.Dark);
        CurrentTheme = theme;
    }
}
