using System.Collections.ObjectModel;
using System.Windows.Threading;
using WPF.Lib.Controls.Abstractions;
using WPF.Lib.Controls.Models;

namespace WPF.Lib.Controls.Services;

public sealed class ToastService : IToastService, IDisposable
{
    private readonly ObservableCollection<ToastNotification> _notifications = [];
    private readonly Dictionary<Guid, TimerRegistration> _timers = [];
    private readonly Dispatcher _dispatcher;
    private bool _disposed;

    public ToastService()
    {
        _dispatcher = Dispatcher.CurrentDispatcher;
        Notifications = new(_notifications);
    }

    public ReadOnlyObservableCollection<ToastNotification> Notifications { get; }
    public int MaximumVisible { get; set; } = 4;

    public ToastNotification Show(string message, ToastType type = ToastType.Information, string? title = null, TimeSpan? duration = null)
    {
        return Invoke(() => ShowCore(message, type, title, duration));
    }

    public void Dismiss(ToastNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        Invoke(() => RemoveCore(notification));
    }

    public void Clear()
    {
        Invoke(ClearCore);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_dispatcher.HasShutdownStarted || _dispatcher.HasShutdownFinished)
        {
            _timers.Clear();
            _disposed = true;
            return;
        }

        Invoke(() =>
        {
            ClearCore();
            _disposed = true;
        });
    }

    private ToastNotification ShowCore(
        string message,
        ToastType type,
        string? title,
        TimeSpan? duration)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var notification = new ToastNotification(Guid.NewGuid(), title ?? GetDefaultTitle(type), message, type, duration ?? TimeSpan.FromSeconds(4));
        _notifications.Insert(0, notification);

        while (_notifications.Count > Math.Max(1, MaximumVisible))
        {
            RemoveCore(_notifications[^1]);
        }

        if (notification.Duration > TimeSpan.Zero)
        {
            var timer = new DispatcherTimer { Interval = notification.Duration };
            EventHandler handler = (_, _) => RemoveCore(notification);
            timer.Tick += handler;
            _timers.Add(notification.Id, new TimerRegistration(timer, handler));
            timer.Start();
        }

        return notification;
    }

    private void RemoveCore(ToastNotification notification)
    {
        if (_timers.Remove(notification.Id, out var registration))
        {
            registration.Timer.Stop();
            registration.Timer.Tick -= registration.Handler;
        }

        _notifications.Remove(notification);
    }

    private void ClearCore()
    {
        foreach (var registration in _timers.Values)
        {
            registration.Timer.Stop();
            registration.Timer.Tick -= registration.Handler;
        }

        _timers.Clear();
        _notifications.Clear();
    }

    private void Invoke(Action action)
    {
        if (_dispatcher.CheckAccess())
        {
            action();
            return;
        }

        _dispatcher.Invoke(action);
    }

    private T Invoke<T>(Func<T> action)
    {
        return _dispatcher.CheckAccess() ? action() : _dispatcher.Invoke(action);
    }

    private static string GetDefaultTitle(ToastType type) => type switch
    {
        ToastType.Success => "성공",
        ToastType.Warning => "주의",
        ToastType.Error => "오류",
        _ => "안내"
    };

    private sealed record TimerRegistration(DispatcherTimer Timer, EventHandler Handler);
}
