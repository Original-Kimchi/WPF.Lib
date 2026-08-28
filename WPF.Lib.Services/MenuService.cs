using WPF.Lib.Core.Abstractions;
using WPF.Lib.Core.Models;

namespace WPF.Lib.Services;

public sealed class MenuService : IMenuService
{
    private readonly IReadOnlyList<MenuItemDefinition> _menus;

    public MenuService(WpfServiceOptions options)
    {
        _menus = options.Menus
            .OrderBy(menu => menu.Order)
            .ToArray();
    }

    public IReadOnlyList<MenuItemDefinition> GetMenus()
    {
        return _menus;
    }
}
