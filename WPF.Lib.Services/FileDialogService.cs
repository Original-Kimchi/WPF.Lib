using Microsoft.Win32;
using WPF.Lib.Core.Abstractions;
using WPF.Lib.Core.Models;

namespace WPF.Lib.Services;

public sealed class FileDialogService : IFileDialogService
{
    public Task<string?> SelectOpenFileAsync(
        FileDialogOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var dialog = new OpenFileDialog
        {
            Title = options.Title,
            Filter = options.Filter,
            InitialDirectory = options.InitialDirectory,
            FileName = options.DefaultFileName ?? string.Empty,
            CheckFileExists = true,
            Multiselect = false
        };

        return Task.FromResult(dialog.ShowDialog() == true ? dialog.FileName : null);
    }

    public Task<string?> SelectSaveFileAsync(
        FileDialogOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var dialog = new SaveFileDialog
        {
            Title = options.Title,
            Filter = options.Filter,
            InitialDirectory = options.InitialDirectory,
            FileName = options.DefaultFileName ?? string.Empty
        };

        return Task.FromResult(dialog.ShowDialog() == true ? dialog.FileName : null);
    }
}
