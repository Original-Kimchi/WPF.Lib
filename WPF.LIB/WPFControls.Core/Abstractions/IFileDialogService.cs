using WPFControls.Core.Models;

namespace WPFControls.Core.Abstractions;

public interface IFileDialogService
{
    Task<string?> SelectOpenFileAsync(
        FileDialogOptions options,
        CancellationToken cancellationToken = default);

    Task<string?> SelectSaveFileAsync(
        FileDialogOptions options,
        CancellationToken cancellationToken = default);
}
