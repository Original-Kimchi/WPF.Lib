using System.Collections.ObjectModel;
using System.Windows.Threading;
using WPF.Lib.Controls.Abstractions;
using WPF.Lib.Controls.Models;

namespace WPF.Lib.Controls.Services;

public sealed class ToastService : IToastService
{
    private readonly ObservableCollection<ToastNotification> _notifications = [];

    public ToastService() => Notifications = new(_notifications);

    public ReadOnlyObservableCollection<ToastNotification> Notifications { get; }
    public int MaximumVisible { get; set; } = 4;

    public ToastNotification Show(string message, ToastType type = ToastType.Information, string? title = null, TimeSpan? duration = null)
    {
        var notification = new ToastNotification(Guid.NewGuid(), title ?? GetDefaultTitle(type), message, type, duration ?? TimeSpan.FromSeconds(4));
        _notifications.Insert(0, notification);

        while (_notifications.Count > Math.Max(1, MaximumVisible))
        {
            _notifications.RemoveAt(_notifications.Count - 1);
        }

        if (notification.Duration > TimeSpan.Zero)
        {
            var timer = new DispatcherTimer { Interval = notification.Duration };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                _notifications.Remove(notification);
            };
            timer.Start();
        }

        return notification;
    }

    public void Dismiss(ToastNotification notification) => _notifications.Remove(notification);
    public void Clear() => _notifications.Clear();

    private static string GetDefaultTitle(ToastType type) => type switch
    {
        ToastType.Success => "성공",
        ToastType.Warning => "주의",
        ToastType.Error => "오류",
        _ => "안내"
    };
}
