using System.IO;
using System.Text.Json;
using WPFControls.UI.Models;

namespace WPFControls.UI.Services;

public sealed class ScheduleStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WPFControls.UI",
        "schedules.json");

    public async Task<IReadOnlyList<ScheduleItem>> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<List<ScheduleItem>>(stream, JsonOptions) ?? [];
    }

    public async Task SaveAsync(IEnumerable<ScheduleItem> schedules)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, schedules, JsonOptions);
    }
}
