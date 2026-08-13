using WPF.Lib.Core.Abstractions;
using WPF.Lib.Core.Models;

namespace WPFControls.UI.Services;

public sealed class MenuService : IMenuService
{
    private static readonly IReadOnlyList<MenuItemDefinition> Menus =
    [
        new(
            Id: "main",
            Title: "Main",
            Route: "main",
            Icon: "ViewDashboardOutline",
            Order: 0),
        new(
            Id: "image-viewer",
            Title: "Image Viewer",
            Route: "image-viewer",
            Icon: "ImageOutline",
            Order: 10)
    ];

    public IReadOnlyList<MenuItemDefinition> GetMenus()
    {
        return Menus.OrderBy(menu => menu.Order).ToArray();
    }
}
