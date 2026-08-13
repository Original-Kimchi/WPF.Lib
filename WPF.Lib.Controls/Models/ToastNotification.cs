namespace WPF.Lib.Controls.Models;

public sealed record ToastNotification(
    Guid Id,
    string Title,
    string Message,
    ToastType Type,
    TimeSpan Duration);
