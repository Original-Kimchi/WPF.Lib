using WPF.Lib.Core.Abstractions;
using WPF.Lib.Core.Models;
using WPF.Lib.MVVM;

namespace WPFControls.UI.ViewModels;

public sealed class ShellViewModel : BaseMainViewModel<MenuItemDefinition>
{
    private readonly MainViewModel _mainViewModel;
    private readonly ScheduleViewModel _scheduleViewModel;
    private readonly ImageViewerViewModel _imageViewerViewModel;
    private readonly DummyJsonViewModel _dummyJsonViewModel;

    public ShellViewModel(
        IMenuService menuService,
        MainViewModel mainViewModel,
        ScheduleViewModel scheduleViewModel,
        ImageViewerViewModel imageViewerViewModel,
        DummyJsonViewModel dummyJsonViewModel)
        : base(menuService.GetMenus())
    {
        _mainViewModel = mainViewModel;
        _scheduleViewModel = scheduleViewModel;
        _imageViewerViewModel = imageViewerViewModel;
        _dummyJsonViewModel = dummyJsonViewModel;
        SelectInitialMenu();
    }

    protected override object? ResolveViewModel(MenuItemDefinition menu)
    {
        return menu.Route switch
        {
            "main" => _mainViewModel,
            "schedule" => _scheduleViewModel,
            "image-viewer" => _imageViewerViewModel,
            "dummy-json" => _dummyJsonViewModel,
            _ => null
        };
    }
}
