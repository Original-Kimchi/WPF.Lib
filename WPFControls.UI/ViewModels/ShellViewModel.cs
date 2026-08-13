using WPF.Lib.Core.Abstractions;
using WPF.Lib.Core.Models;
using WPF.Lib.MVVM;

namespace WPFControls.UI.ViewModels;

public sealed class ShellViewModel : BaseMainViewModel<MenuItemDefinition>
{
    private readonly MainViewModel _mainViewModel;

    public ShellViewModel(IMenuService menuService, MainViewModel mainViewModel)
        : base(menuService.GetMenus())
    {
        _mainViewModel = mainViewModel;
        SelectInitialMenu();
    }

    protected override object? ResolveViewModel(MenuItemDefinition menu)
    {
        return menu.Route == "main" ? _mainViewModel : null;
    }
}
