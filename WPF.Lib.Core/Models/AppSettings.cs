namespace WPF.Lib.Core.Models;

public sealed class AppSettings
{
    public ApplicationTheme Theme { get; set; } = ApplicationTheme.Light;

    public double? WindowLeft { get; set; }

    public double? WindowTop { get; set; }

    public double WindowWidth { get; set; } = 1280;

    public double WindowHeight { get; set; } = 820;

    public bool IsWindowMaximized { get; set; }
}
