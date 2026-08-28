using WPF.Lib.Core.Models;

namespace WPF.Lib.Services;

public sealed class WpfServiceOptions
{
    public string ApplicationName { get; set; } = string.Empty;

    public string SettingsFileName { get; set; } = "settings.json";

    public ICollection<MenuItemDefinition> Menus { get; } = [];
}
