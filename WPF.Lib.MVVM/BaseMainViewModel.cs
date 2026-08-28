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
        protected set
        {
            if (ReferenceEquals(_currentViewModel, value))
            {
                return;
            }

            if (_currentViewModel is INavigationAware current)
            {
                current.OnDeactivated();
            }

            if (SetProperty(ref _currentViewModel, value) && value is INavigationAware next)
            {
                next.OnActivated();
            }
        }
    }

    protected void SelectInitialMenu()
    {
        SelectedMenu = MenuItems.FirstOrDefault();
    }

    protected abstract object? ResolveViewModel(TMenu menu);
}
