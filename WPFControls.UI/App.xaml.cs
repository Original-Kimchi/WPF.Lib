using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WPF.Lib.Core.Abstractions;
using WPF.Lib.Logging.Log4Net;
using WPFControls.UI.Services;
using WPFControls.UI.ViewModels;

namespace WPFControls.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private ILogger<App>? _logger;

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

        _logger = _serviceProvider.GetRequiredService<ILogger<App>>();
        RegisterGlobalExceptionHandlers();
        _logger.LogInformation("애플리케이션을 시작합니다.");

        MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _logger?.LogInformation("애플리케이션을 종료합니다.");
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddLog4Net(options =>
            {
                options.ConfigFilePath = Path.Combine(AppContext.BaseDirectory, "log4net.config");
                options.LogDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "WPFControls.UI",
                    "Logs");
            });
        });

        services.AddSingleton<IMenuService, MenuService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IThemeService, ThemeService>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ShellViewModel>();

        services.AddSingleton<MainWindow>();
    }

    private void RegisterGlobalExceptionHandlers()
    {
        DispatcherUnhandledException += (_, args) =>
        {
            _logger?.LogCritical(args.Exception, "UI 스레드에서 처리되지 않은 예외가 발생했습니다.");
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            _logger?.LogCritical(
                args.ExceptionObject as Exception,
                "처리되지 않은 애플리케이션 예외가 발생했습니다. 종료 여부: {IsTerminating}",
                args.IsTerminating);
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            _logger?.LogError(args.Exception, "관찰되지 않은 Task 예외가 발생했습니다.");
            args.SetObserved();
        };
    }
}
