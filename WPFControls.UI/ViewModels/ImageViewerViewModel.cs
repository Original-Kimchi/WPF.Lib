using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using WPF.Lib.Controls.DragDrop;
using WPF.Lib.Controls.Models;
using WPF.Lib.Core.Abstractions;
using WPF.Lib.Core.Models;
using WPF.Lib.MVVM;
using WPF.Lib.MVVM.Commands;

namespace WPFControls.UI.ViewModels;

public sealed class ImageViewerViewModel : BaseViewModel, IDisposable
{
    private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".bmp",
        ".gif",
        ".tif",
        ".tiff"
    };

    private readonly IFileDialogService _fileDialogService;
    private readonly ILogger<ImageViewerViewModel> _logger;
    private string? _imagePath;
    private string _statusMessage = "상단의 이미지 열기 버튼으로 파일을 선택하세요.";
    private bool _disposed;

    public ImageViewerViewModel(
        IFileDialogService fileDialogService,
        ILogger<ImageViewerViewModel> logger)
    {
        _fileDialogService = fileDialogService;
        _logger = logger;
        OpenImageCommand = new RelayCommand(OpenImage);
        DropImageCommand = new RelayCommand(DropImage, CanDropImage);
    }

    public ImageModel Image { get; } = new();

    public string? ImagePath
    {
        get => _imagePath;
        private set => SetProperty(ref _imagePath, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public RelayCommand OpenImageCommand { get; }

    public RelayCommand DropImageCommand { get; }

    private async void OpenImage()
    {
        var path = await _fileDialogService.SelectOpenFileAsync(new FileDialogOptions(
            "이미지 파일 선택",
            "이미지 파일|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|모든 파일|*.*",
            GetInitialDirectory()));

        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        LoadImageFile(path);
    }

    public bool CanDropFiles(IEnumerable<string>? paths)
    {
        return paths?.Any(IsSupportedImageFile) == true;
    }

    public void DropFiles(IEnumerable<string>? paths)
    {
        var path = paths?.FirstOrDefault(IsSupportedImageFile);
        if (path is null)
        {
            StatusMessage = "지원되는 이미지 파일을 드롭해 주세요.";
            return;
        }

        LoadImageFile(path);
    }

    private bool CanDropImage(object? parameter)
    {
        return parameter is DragDropInfo info && CanDropFiles(info.Files);
    }

    private void DropImage(object? parameter)
    {
        if (parameter is DragDropInfo info)
        {
            DropFiles(info.Files);
        }
    }

    private void LoadImageFile(string path)
    {
        try
        {
            var source = LoadImage(path);

            Image.SelectedAnnotation = null;
            Image.Annotations.Clear();
            Image.Source = source;
            Image.Scale = 1;
            ImagePath = path;
            StatusMessage = $"{source.PixelWidth} × {source.PixelHeight} px";
            _logger.LogInformation("이미지를 불러왔습니다. Path: {ImagePath}", path);
        }
        catch (Exception exception)
        {
            StatusMessage = "이미지 파일을 불러올 수 없습니다.";
            _logger.LogError(exception, "이미지 파일 로드에 실패했습니다. Path: {ImagePath}", path);
        }
    }

    private string? GetInitialDirectory()
    {
        return string.IsNullOrWhiteSpace(ImagePath) ? null : Path.GetDirectoryName(ImagePath);
    }

    private static BitmapImage LoadImage(string path)
    {
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            FileOptions.SequentialScan);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Image.Clear();
        ImagePath = null;
    }

    private static bool IsSupportedImageFile(string path)
    {
        return File.Exists(path) && SupportedImageExtensions.Contains(Path.GetExtension(path));
    }
}
