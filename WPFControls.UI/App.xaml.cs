using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WPF.Lib.Core.Abstractions;
using WPFControls.UI.Services;
using WPFControls.UI.ViewModels;

namespace WPFControls.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMenuService, MenuService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IThemeService, ThemeService>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ShellViewModel>();

        services.AddSingleton<MainWindow>();
    }
}
