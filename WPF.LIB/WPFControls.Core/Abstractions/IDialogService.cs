namespace WPFControls.Core.Abstractions;

public interface IDialogService
{
    Task ShowMessageAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default);

    Task<bool> ShowConfirmationAsync(
        string title,
        string message,
        CancellationToken cancellationToken = default);
}
