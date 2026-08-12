using WPFControls.Core.Models;

namespace WPFControls.Core.Abstractions;

public interface IThemeService
{
    ApplicationTheme CurrentTheme { get; }

    void ApplyTheme(ApplicationTheme theme);
}
