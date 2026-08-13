using WPF.Lib.Core.Abstractions;
using WPF.Lib.Core.Models;
using WPF.Lib.MVVM;
using WPF.Lib.MVVM.Commands;
using Microsoft.Extensions.Logging;
using WPF.Lib.Controls.Abstractions;
using WPF.Lib.Controls.Models;

namespace WPFControls.UI.ViewModels;

public sealed class MainViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IThemeService _themeService;
    private readonly ILogger<MainViewModel> _logger;
    private readonly IToastService _toastService;
    private ApplicationTheme _selectedTheme = ApplicationTheme.Light;
    private string _dialogResult = "결과: 아직 실행하지 않음";

    public MainViewModel(
        IDialogService dialogService,
        IThemeService themeService,
        IToastService toastService,
        ILogger<MainViewModel> logger)
    {
        _dialogService = dialogService;
        _themeService = themeService;
        _toastService = toastService;
        _logger = logger;
        ShowDialogCommand = new RelayCommand(ShowDialog);
        ShowInfoToastCommand = new RelayCommand(() => _toastService.Show("새로운 업데이트를 확인할 수 있습니다."));
        ShowSuccessToastCommand = new RelayCommand(() => _toastService.Show("변경 사항이 저장되었습니다.", ToastType.Success));
        ShowWarningToastCommand = new RelayCommand(() => _toastService.Show("저장 공간이 얼마 남지 않았습니다.", ToastType.Warning, duration: TimeSpan.FromSeconds(6)));
        ShowErrorToastCommand = new RelayCommand(() => _toastService.Show("요청을 처리하지 못했습니다.", ToastType.Error, duration: TimeSpan.Zero));
    }

    public IReadOnlyList<ApplicationTheme> Themes { get; } = Enum.GetValues<ApplicationTheme>();

    public ApplicationTheme SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value))
            {
                _themeService.ApplyTheme(value);
                _logger.LogInformation("테마를 변경했습니다. Theme: {Theme}", value);
            }
        }
    }

    public string DialogResult
    {
        get => _dialogResult;
        private set => SetProperty(ref _dialogResult, value);
    }

    public RelayCommand ShowDialogCommand { get; }
    public RelayCommand ShowInfoToastCommand { get; }
    public RelayCommand ShowSuccessToastCommand { get; }
    public RelayCommand ShowWarningToastCommand { get; }
    public RelayCommand ShowErrorToastCommand { get; }
    public IToastService ToastService => _toastService;

    public IReadOnlyList<Member> Members { get; } =
    [
        new("AK", "Alex Kim", "alex@example.com", "Designer"),
        new("JL", "Jamie Lee", "jamie@example.com", "Developer"),
        new("SP", "Sam Park", "sam@example.com", "Product Manager"),
        new("MC", "Morgan Choi", "morgan@example.com", "QA Engineer")
    ];

    public IReadOnlyList<Order> Orders { get; } =
    [
        new("#1048", "Acme Studio", "2026-08-12", "Completed", "$1,240"),
        new("#1047", "Northwind", "2026-08-11", "Processing", "$860"),
        new("#1046", "Contoso", "2026-08-10", "Completed", "$2,150"),
        new("#1045", "Fabrikam", "2026-08-09", "Pending", "$540"),
        new("#1044", "Adventure Works", "2026-08-08", "Completed", "$975")
    ];

    private async void ShowDialog()
    {
        _logger.LogInformation("확인 다이얼로그를 표시합니다.");
        var confirmed = await _dialogService.ShowConfirmationAsync(
            "변경 사항 확인",
            "선택한 설정을 적용합니다. 계속 진행하려면 확인을 눌러주세요.");
        DialogResult = confirmed ? "결과: 확인" : "결과: 취소 또는 닫기";
    }

    public sealed record Member(string Initials, string Name, string Email, string Role);
    public sealed record Order(string Id, string Customer, string Date, string Status, string Total);
}
