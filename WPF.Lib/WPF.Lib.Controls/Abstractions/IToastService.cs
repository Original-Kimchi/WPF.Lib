using System.Collections.ObjectModel;
using WPF.Lib.Controls.Models;

namespace WPF.Lib.Controls.Abstractions;

public interface IToastService
{
    ReadOnlyObservableCollection<ToastNotification> Notifications { get; }
    ToastNotification Show(string message, ToastType type = ToastType.Information, string? title = null, TimeSpan? duration = null);
    void Dismiss(ToastNotification notification);
    void Clear();
}
