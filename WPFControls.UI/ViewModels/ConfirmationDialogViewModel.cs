using WPF.Lib.MVVM.Dialogs;

namespace WPFControls.UI.ViewModels;

public sealed class ConfirmationDialogViewModel : BaseDialogViewModel
{
    public ConfirmationDialogViewModel(
        string title,
        string message,
        DialogButtonMode buttonMode = DialogButtonMode.ConfirmCancel)
    {
        Title = title;
        Message = message;
        ButtonMode = buttonMode;
    }

    public string Message { get; }
}
