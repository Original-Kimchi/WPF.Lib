using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WPF.Lib.Core.Abstractions;
using WPF.Lib.Core.Models;

namespace WPF.Lib.Services;

public sealed class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly ILogger<SettingsService> _logger;
    private readonly string _settingsPath;

    public SettingsService(
        WpfServiceOptions options,
        ILogger<SettingsService> logger)
    {
        _logger = logger;
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            options.ApplicationName,
            options.SettingsFileName);
    }

    public AppSettings Current { get; private set; } = new();

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_settingsPath))
        {
            return;
        }

        try
        {
            using var stream = File.OpenRead(_settingsPath);
            Current = await JsonSerializer.DeserializeAsync<AppSettings>(
                stream,
                SerializerOptions,
                cancellationToken).ConfigureAwait(false) ?? new AppSettings();
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "설정 파일 형식이 올바르지 않아 기본 설정을 사용합니다.");
            Current = new AppSettings();
        }
        catch (IOException exception)
        {
            _logger.LogWarning(exception, "설정 파일을 읽지 못해 기본 설정을 사용합니다.");
            Current = new AppSettings();
        }
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath)!;
            Directory.CreateDirectory(directory);

            using var stream = File.Create(_settingsPath);
            await JsonSerializer.SerializeAsync(
                stream,
                Current,
                SerializerOptions,
                cancellationToken).ConfigureAwait(false);
        }
        catch (IOException exception)
        {
            _logger.LogError(exception, "설정 파일을 저장하지 못했습니다.");
        }
    }
}
