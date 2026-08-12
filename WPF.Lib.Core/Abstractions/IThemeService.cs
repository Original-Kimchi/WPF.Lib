using WPF.Lib.Core.Models;

namespace WPF.Lib.Core.Abstractions;

public interface IThemeService
{
    ApplicationTheme CurrentTheme { get; }

    void ApplyTheme(ApplicationTheme theme);
}
