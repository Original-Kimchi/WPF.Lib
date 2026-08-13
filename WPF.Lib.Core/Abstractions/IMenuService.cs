using WPF.Lib.Core.Models;

namespace WPF.Lib.Core.Abstractions;

public interface IMenuService
{
    IReadOnlyList<MenuItemDefinition> GetMenus();
}
