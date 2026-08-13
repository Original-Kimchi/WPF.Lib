using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using WPF.Lib.Controls.Models;
using WPF.Lib.Core.Abstractions;
using WPF.Lib.Core.Models;
using WPF.Lib.MVVM;
using WPF.Lib.MVVM.Commands;

namespace WPFControls.UI.ViewModels;

public sealed class ImageViewerViewModel : BaseViewModel
{
    private readonly IFileDialogService _fileDialogService;
    private readonly ILogger<ImageViewerViewModel> _logger;
    private string? _imagePath;
    private string _statusMessage = "상단의 이미지 열기 버튼으로 파일을 선택하세요.";

    public ImageViewerViewModel(
        IFileDialogService fileDialogService,
        ILogger<ImageViewerViewModel> logger)
    {
        _fileDialogService = fileDialogService;
        _logger = logger;
        OpenImageCommand = new RelayCommand(OpenImage);
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

        try
        {
            Image.Source = LoadImage(path);
            Image.Scale = 1;
            Image.Annotations.Clear();
            Image.SelectedAnnotation = null;
            ImagePath = path;
            StatusMessage = $"{Image.Source.Width:0} × {Image.Source.Height:0} px";
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
        using var stream = File.OpenRead(path);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }
}
