using WPF.Lib.MVVM.Dialogs;

namespace WPF.Lib.Services.Dialogs;

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
