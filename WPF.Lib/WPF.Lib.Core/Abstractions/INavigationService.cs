namespace WPF.Lib.Core.Abstractions;

public interface INavigationService
{
    bool CanGoBack { get; }

    Task NavigateToAsync<TViewModel>(
        object? parameter = null,
        CancellationToken cancellationToken = default)
        where TViewModel : class;

    Task GoBackAsync(CancellationToken cancellationToken = default);
}
