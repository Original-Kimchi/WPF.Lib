using WPF.Lib.Core.Abstractions;
using WPF.Lib.Core.Models;
using WPF.Lib.MVVM;

namespace WPFControls.UI.ViewModels;

public sealed class ShellViewModel : BaseMainViewModel<MenuItemDefinition>
{
    private readonly MainViewModel _mainViewModel;
    private readonly ImageViewerViewModel _imageViewerViewModel;

    public ShellViewModel(
        IMenuService menuService,
        MainViewModel mainViewModel,
        ImageViewerViewModel imageViewerViewModel)
        : base(menuService.GetMenus())
    {
        _mainViewModel = mainViewModel;
        _imageViewerViewModel = imageViewerViewModel;
        SelectInitialMenu();
    }

    protected override object? ResolveViewModel(MenuItemDefinition menu)
    {
        return menu.Route switch
        {
            "main" => _mainViewModel,
            "image-viewer" => _imageViewerViewModel,
            _ => null
        };
    }
}
