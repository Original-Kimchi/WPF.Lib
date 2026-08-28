using System.IO;
using Microsoft.Extensions.DependencyInjection;
using WPF.Lib.Core.Abstractions;
using WPF.Lib.Services.Abstractions;

namespace WPF.Lib.Services;

public static class WpfServiceCollectionExtensions
{
    public static IServiceCollection AddWpfServices(
        this IServiceCollection services,
        Action<WpfServiceOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new WpfServiceOptions();
        configure(options);
        Validate(options);

        services.AddSingleton(options);
        services.AddSingleton<IMenuService, MenuService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IMessageBoxService, MessageBoxService>();
        services.AddSingleton<IFileDialogService, FileDialogService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IAppLifetimeService, AppLifetimeService>();
        services.AddSingleton<IExceptionHandler, LoggingExceptionHandler>();

        return services;
    }

    private static void Validate(WpfServiceOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ApplicationName);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.SettingsFileName);

        if (options.ApplicationName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException(
                "Application name contains invalid path characters.",
                nameof(options));
        }

        if (!string.Equals(
                options.SettingsFileName,
                Path.GetFileName(options.SettingsFileName),
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Settings file name must not contain a directory path.",
                nameof(options));
        }
    }
}
