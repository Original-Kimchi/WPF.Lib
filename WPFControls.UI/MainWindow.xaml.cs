using System.Windows;
using WPF.Lib.Core.Abstractions;
using WPFControls.UI.ViewModels;

namespace WPFControls.UI;

public partial class MainWindow : Window
{
    private readonly ISettingsService _settingsService;

    public MainWindow(ShellViewModel viewModel, ISettingsService settingsService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _settingsService = settingsService;
        RestoreWindowSettings();
        Closing += OnClosing;
    }

    private void RestoreWindowSettings()
    {
        var settings = _settingsService.Current;
        Width = Math.Max(MinWidth, settings.WindowWidth);
        Height = Math.Max(MinHeight, settings.WindowHeight);

        if (settings.WindowLeft is double left && settings.WindowTop is double top)
        {
            var savedBounds = new Rect(left, top, Width, Height);
            var virtualScreen = new Rect(
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight);

            if (savedBounds.IntersectsWith(virtualScreen))
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = left;
                Top = top;
            }
        }

        if (settings.IsWindowMaximized)
        {
            WindowState = WindowState.Maximized;
        }
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        var bounds = RestoreBounds;
        var settings = _settingsService.Current;
        settings.WindowLeft = bounds.Left;
        settings.WindowTop = bounds.Top;
        settings.WindowWidth = bounds.Width;
        settings.WindowHeight = bounds.Height;
        settings.IsWindowMaximized = WindowState == WindowState.Maximized;
    }
}
