using System.Windows;
using WPF.Lib.Core.Abstractions;
using WPF.Lib.MVVM.Dialogs;
using WPFControls.UI.Dialogs;
using WPFControls.UI.ViewModels;

namespace WPFControls.UI.Services;

public sealed class DialogService : IDialogService
{
    public Task ShowMessageAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ShowDialog(title, message, DialogButtonMode.Confirm);
        return Task.CompletedTask;
    }

    public Task<bool> ShowConfirmationAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ShowDialog(title, message, DialogButtonMode.ConfirmCancel) == DialogOutcome.Confirmed);
    }

    private static DialogOutcome? ShowDialog(string title, string message, DialogButtonMode buttonMode)
    {
        var dialog = new ConfirmationDialog
        {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
                ?? Application.Current.MainWindow,
            DataContext = new ConfirmationDialogViewModel(title, message, buttonMode)
        };

        dialog.ShowDialog();
        return dialog.Outcome;
    }
}
