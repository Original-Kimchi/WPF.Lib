using WPF.Lib.Core.Models;

namespace WPF.Lib.Core.Abstractions;

public interface ISettingsService
{
    AppSettings Current { get; }

    Task LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(CancellationToken cancellationToken = default);
}
