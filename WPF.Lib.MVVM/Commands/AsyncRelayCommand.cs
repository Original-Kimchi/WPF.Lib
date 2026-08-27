using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace WPF.Lib.MVVM.Commands;

public sealed class AsyncRelayCommand : ICommand, INotifyPropertyChanged
{
    private readonly Func<object?, CancellationToken, Task> _execute;
    private readonly Predicate<object?>? _canExecute;
    private CancellationTokenSource? _executionCancellationSource;
    private bool _isRunning;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        : this(
            (_, _) => execute(),
            canExecute is null ? null : _ => canExecute())
    {
        ArgumentNullException.ThrowIfNull(execute);
    }

    public AsyncRelayCommand(
        Func<object?, Task> execute,
        Predicate<object?>? canExecute = null)
        : this((parameter, _) => execute(parameter), canExecute)
    {
        ArgumentNullException.ThrowIfNull(execute);
    }

    public AsyncRelayCommand(
        Func<object?, CancellationToken, Task> execute,
        Predicate<object?>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);

        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (_isRunning == value)
            {
                return;
            }

            _isRunning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanBeCanceled));
            NotifyCanExecuteChanged();
        }
    }

    public bool CanBeCanceled => IsRunning &&
                                 _executionCancellationSource is { IsCancellationRequested: false };

    public bool CanExecute(object? parameter)
    {
        return !IsRunning && (_canExecute?.Invoke(parameter) ?? true);
    }

    public async void Execute(object? parameter)
    {
        await ExecuteAsync(parameter);
    }

    public async Task ExecuteAsync(
        object? parameter = null,
        CancellationToken cancellationToken = default)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        using var executionCancellationSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executionCancellationSource = executionCancellationSource;
        IsRunning = true;

        try
        {
            await _execute(parameter, executionCancellationSource.Token);
        }
        finally
        {
            IsRunning = false;
            _executionCancellationSource = null;
            OnPropertyChanged(nameof(CanBeCanceled));
        }
    }

    public void Cancel()
    {
        _executionCancellationSource?.Cancel();
        OnPropertyChanged(nameof(CanBeCanceled));
    }

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
