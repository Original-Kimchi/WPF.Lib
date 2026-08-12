using WPF.Lib.Core.Models;

namespace WPF.Lib.Core.Abstractions;

public interface IFileDialogService
{
    Task<string?> SelectOpenFileAsync(
        FileDialogOptions options,
        CancellationToken cancellationToken = default);

    Task<string?> SelectSaveFileAsync(
        FileDialogOptions options,
        CancellationToken cancellationToken = default);
}
