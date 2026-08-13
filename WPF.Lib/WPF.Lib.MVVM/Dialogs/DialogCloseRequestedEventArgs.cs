namespace WPF.Lib.MVVM.Dialogs;

public sealed class DialogCloseRequestedEventArgs(DialogOutcome outcome)
    : EventArgs
{
    public DialogOutcome Outcome { get; } = outcome;
}
