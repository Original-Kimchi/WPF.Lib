namespace WPF.Lib.Core.Models;

public sealed record MenuItemDefinition(
    string Id,
    string Title,
    string Route,
    string? Icon = null,
    int Order = 0,
    IReadOnlyList<MenuItemDefinition>? Children = null)
{
    public IReadOnlyList<MenuItemDefinition> Items { get; } = Children ?? [];
}
