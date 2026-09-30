using WPF.Lib.MVVM.Commands;

namespace WPF.Lib.MVVM.Dialogs;

public abstract class BaseDialogViewModel : BaseViewModel
{
    private string _title = string.Empty;
    private string _confirmText = "확인";
    private string _cancelText = "취소";
    private DialogButtonMode _buttonMode = DialogButtonMode.ConfirmCancel;

    protected BaseDialogViewModel()
    {
        ConfirmCommand = new RelayCommand(OnConfirm, CanConfirm);
        CancelCommand = new RelayCommand(OnCancel);
    }

    public event EventHandler<DialogCloseRequestedEventArgs>? CloseRequested;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string ConfirmText
    {
        get => _confirmText;
        set => SetProperty(ref _confirmText, value);
    }

    public string CancelText
    {
        get => _cancelText;
        set => SetProperty(ref _cancelText, value);
    }

    public DialogButtonMode ButtonMode
    {
        get => _buttonMode;
        set
        {
            if (!SetProperty(ref _buttonMode, value))
            {
                return;
            }

            OnPropertyChanged(nameof(AreActionButtonsVisible));
            OnPropertyChanged(nameof(IsCancelVisible));
        }
    }

    public bool AreActionButtonsVisible => ButtonMode != DialogButtonMode.None;

    public bool IsCancelVisible => ButtonMode == DialogButtonMode.ConfirmCancel;

    public RelayCommand ConfirmCommand { get; }

    public RelayCommand CancelCommand { get; }

    protected virtual bool CanConfirm()
    {
        return true;
    }

    protected virtual void OnConfirm()
    {
        RequestClose(DialogOutcome.Confirmed);
    }

    protected virtual void OnCancel()
    {
        RequestClose(DialogOutcome.Cancelled);
    }

    protected void RefreshConfirmCommand()
    {
        ConfirmCommand.NotifyCanExecuteChanged();
    }

    protected void RequestClose(DialogOutcome outcome)
    {
        CloseRequested?.Invoke(
            this,
            new DialogCloseRequestedEventArgs(outcome));
    }
}
