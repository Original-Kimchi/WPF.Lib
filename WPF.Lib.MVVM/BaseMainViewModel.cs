using WPF.Lib.MVVM.Commands;

namespace WPF.Lib.MVVM;

public abstract class BaseMainViewModel<TMenu> : BaseViewModel where TMenu : class
{
    private TMenu? _selectedMenu;
    private object? _currentViewModel;

    protected BaseMainViewModel(IReadOnlyList<TMenu> menuItems)
    {
        MenuItems = menuItems;
    }

    public IReadOnlyList<TMenu> MenuItems { get; }

    public TMenu? SelectedMenu
    {
        get => _selectedMenu;
        set
        {
            if (SetProperty(ref _selectedMenu, value) && value is not null)
            {
                CurrentViewModel = ResolveViewModel(value);
            }
        }
    }

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        protected set => SetProperty(ref _currentViewModel, value);
    }

    protected void SelectInitialMenu()
    {
        SelectedMenu = MenuItems.FirstOrDefault();
    }

    protected abstract object? ResolveViewModel(TMenu menu);
}
