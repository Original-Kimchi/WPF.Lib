using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WPF.Lib.Api;
using WPF.Lib.Core.Abstractions;
using WPF.Lib.Logging.Log4Net;
using WPF.Lib.Controls.Abstractions;
using WPF.Lib.Controls.Services;
using WPFControls.UI.Services;
using WPFControls.UI.ViewModels;

namespace WPFControls.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private ILogger<App>? _logger;
    private ISettingsService? _settingsService;
    private IExceptionHandler? _exceptionHandler;

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
        _settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
        _logger?.LogInformation("Setting Service 호출");
        _settingsService.LoadAsync().GetAwaiter().GetResult();
        _logger?.LogInformation("Setting Service 호출 완료");
        _serviceProvider.GetRequiredService<IThemeService>().ApplyTheme(_settingsService.Current.Theme);
        RegisterGlobalExceptionHandlers();
        _logger?.LogInformation("애플리케이션을 시작합니다.");

        MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            _logger?.LogInformation("애플리케이션을 종료합니다.");
            _logger?.LogInformation("Setting Service 호출");
            _settingsService?.SaveAsync().GetAwaiter().GetResult();
            _logger?.LogInformation("Setting Service 호출 완료");
        }
        finally
        {
            UnregisterGlobalExceptionHandlers();
            MainWindow = null;
            _serviceProvider?.Dispose();
            _serviceProvider = null;
            _settingsService = null;
            _logger = null;
            base.OnExit(e);
        }
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
        services.AddSingleton<IFileDialogService, FileDialogService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IAppLifetimeService, AppLifetimeService>();
        services.AddSingleton<IExceptionHandler, ExceptionHandler>();
        services.AddSingleton<IToastService, ToastService>();
        services.AddSingleton<ScheduleStorageService>();
        services.AddSingleton<DummyJsonProductService>();
        services.AddApiClient(options =>
        {
            options.BaseAddress = new Uri("https://dummyjson.com/");
            options.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ScheduleViewModel>();
        services.AddSingleton<ImageViewerViewModel>();
        services.AddSingleton<DummyJsonViewModel>();
        services.AddSingleton<ShellViewModel>();

        services.AddSingleton<MainWindow>();
    }

    private void RegisterGlobalExceptionHandlers()
    {
        _exceptionHandler = _serviceProvider!.GetRequiredService<IExceptionHandler>();
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void UnregisterGlobalExceptionHandlers()
    {
        DispatcherUnhandledException -= OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException -= OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
        _exceptionHandler = null;
    }

    private void OnDispatcherUnhandledException(
        object sender,
        System.Windows.Threading.DispatcherUnhandledExceptionEventArgs args)
    {
        _exceptionHandler?.Handle(args.Exception, "Dispatcher");
    }

    private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        var exception = args.ExceptionObject as Exception
            ?? new InvalidOperationException(args.ExceptionObject?.ToString());
        _exceptionHandler?.Handle(exception, "AppDomain", args.IsTerminating);
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs args)
    {
        _exceptionHandler?.Handle(args.Exception, "TaskScheduler");
        args.SetObserved();
    }
}
