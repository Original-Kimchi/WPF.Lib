namespace WPFControls.Core.Models;

public sealed record FileDialogOptions(
    string Title,
    string Filter,
    string? InitialDirectory = null,
    string? DefaultFileName = null);
