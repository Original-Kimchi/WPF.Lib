namespace WPF.Lib.MVVM;

public interface INavigationAware
{
    void OnActivated();

    void OnDeactivated();
}
