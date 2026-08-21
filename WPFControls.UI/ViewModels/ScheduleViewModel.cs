using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using WPF.Lib.Controls.Abstractions;
using WPF.Lib.Controls.Models;
using WPF.Lib.Core.Abstractions;
using WPF.Lib.Core.Models;
using WPF.Lib.MVVM;
using WPF.Lib.MVVM.Commands;
using WPFControls.UI.Models;
using WPFControls.UI.Services;

namespace WPFControls.UI.ViewModels;

public sealed class ScheduleViewModel : BaseViewModel
{
    private readonly IThemeService _themeService;
    private readonly ISettingsService _settingsService;
    private readonly IToastService _toastService;
    private readonly ScheduleStorageService _storageService;
    private readonly ILogger<ScheduleViewModel> _logger;
    private DateTime _selectedDate = DateTime.Today;
    private DateTime _displayDate = DateTime.Today;
    private ScheduleItem? _selectedSchedule;
    private ApplicationTheme _selectedTheme;
    private string _editorTitle = string.Empty;
    private DateTime _editorDate = DateTime.Today;
    private TimeSpan? _editorStartTime = new(9, 0, 0);
    private TimeSpan? _editorEndTime = new(10, 0, 0);
    private string _editorCategory = "업무";
    private string _editorNotes = string.Empty;
    private bool _isEditing;

    public ScheduleViewModel(
        IThemeService themeService,
        ISettingsService settingsService,
        IToastService toastService,
        ScheduleStorageService storageService,
        ILogger<ScheduleViewModel> logger)
    {
        _themeService = themeService;
        _settingsService = settingsService;
        _toastService = toastService;
        _storageService = storageService;
        _logger = logger;
        _selectedTheme = themeService.CurrentTheme;

        PreviousMonthCommand = new RelayCommand(() => DisplayDate = DisplayDate.AddMonths(-1));
        NextMonthCommand = new RelayCommand(() => DisplayDate = DisplayDate.AddMonths(1));
        TodayCommand = new RelayCommand(() =>
        {
            SelectedDate = DateTime.Today;
            DisplayDate = DateTime.Today;
        });
        NewScheduleCommand = new RelayCommand(BeginNewSchedule);
        EditScheduleCommand = new RelayCommand(BeginEditSchedule, () => SelectedSchedule is not null);
        SaveScheduleCommand = new RelayCommand(SaveSchedule);
        DeleteScheduleCommand = new RelayCommand(DeleteSchedule, () => SelectedSchedule is not null);
        ToggleCompletedCommand = new RelayCommand(ToggleCompleted);

        LoadSchedules();
    }

    public ObservableCollection<ScheduleItem> Schedules { get; } = [];
    public ObservableCollection<ScheduleItem> DailySchedules { get; } = [];
    public IReadOnlyList<string> Categories { get; } = ["업무", "개인", "미팅", "기념일"];
    public IReadOnlyList<ApplicationTheme> Themes { get; } = Enum.GetValues<ApplicationTheme>();
    public IToastService ToastService => _toastService;

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value.Date))
            {
                OnPropertyChanged(nameof(SelectedDateTitle));
                RefreshDailySchedules();
            }
        }
    }

    public DateTime DisplayDate
    {
        get => _displayDate;
        set
        {
            if (SetProperty(ref _displayDate, value))
            {
                OnPropertyChanged(nameof(DisplayMonthTitle));
            }
        }
    }

    public string DisplayMonthTitle => DisplayDate.ToString("yyyy년 M월");
    public string SelectedDateTitle => SelectedDate.ToString("M월 d일 dddd");
    public string ScheduleCountText => $"{DailySchedules.Count}개의 일정";

    public ScheduleItem? SelectedSchedule
    {
        get => _selectedSchedule;
        set
        {
            if (SetProperty(ref _selectedSchedule, value))
            {
                EditScheduleCommand.NotifyCanExecuteChanged();
                DeleteScheduleCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public ApplicationTheme SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value))
            {
                _themeService.ApplyTheme(value);
                _settingsService.Current.Theme = value;
            }
        }
    }

    public string EditorHeading => IsEditing ? "일정 수정" : "새 일정";

    public bool IsEditing
    {
        get => _isEditing;
        private set
        {
            if (SetProperty(ref _isEditing, value))
            {
                OnPropertyChanged(nameof(EditorHeading));
            }
        }
    }

    public string EditorTitle { get => _editorTitle; set => SetProperty(ref _editorTitle, value); }
    public DateTime EditorDate { get => _editorDate; set => SetProperty(ref _editorDate, value.Date); }
    public TimeSpan? EditorStartTime { get => _editorStartTime; set => SetProperty(ref _editorStartTime, value); }
    public TimeSpan? EditorEndTime { get => _editorEndTime; set => SetProperty(ref _editorEndTime, value); }
    public string EditorCategory { get => _editorCategory; set => SetProperty(ref _editorCategory, value); }
    public string EditorNotes { get => _editorNotes; set => SetProperty(ref _editorNotes, value); }

    public RelayCommand PreviousMonthCommand { get; }
    public RelayCommand NextMonthCommand { get; }
    public RelayCommand TodayCommand { get; }
    public RelayCommand NewScheduleCommand { get; }
    public RelayCommand EditScheduleCommand { get; }
    public RelayCommand SaveScheduleCommand { get; }
    public RelayCommand DeleteScheduleCommand { get; }
    public RelayCommand ToggleCompletedCommand { get; }

    private async void LoadSchedules()
    {
        try
        {
            var items = await _storageService.LoadAsync();
            if (items.Count == 0)
            {
                items = CreateSampleSchedules();
            }

            foreach (var item in items.OrderBy(item => item.Date).ThenBy(item => item.StartTime))
            {
                Schedules.Add(item);
            }

            RefreshDailySchedules();
            BeginNewSchedule();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "일정 데이터를 불러오지 못했습니다.");
            _toastService.Show("일정 데이터를 불러오지 못했습니다.", ToastType.Error);
        }
    }

    private void RefreshDailySchedules()
    {
        DailySchedules.Clear();
        foreach (var schedule in Schedules
                     .Where(item => item.Date.Date == SelectedDate.Date)
                     .OrderBy(item => item.StartTime))
        {
            DailySchedules.Add(schedule);
        }

        SelectedSchedule = null;
        OnPropertyChanged(nameof(ScheduleCountText));
    }

    private void BeginNewSchedule()
    {
        IsEditing = false;
        EditorTitle = string.Empty;
        EditorDate = SelectedDate;
        EditorStartTime = new TimeSpan(9, 0, 0);
        EditorEndTime = new TimeSpan(10, 0, 0);
        EditorCategory = Categories[0];
        EditorNotes = string.Empty;
    }

    private void BeginEditSchedule()
    {
        if (SelectedSchedule is not { } schedule)
        {
            return;
        }

        IsEditing = true;
        EditorTitle = schedule.Title;
        EditorDate = schedule.Date;
        EditorStartTime = schedule.StartTime;
        EditorEndTime = schedule.EndTime;
        EditorCategory = schedule.Category;
        EditorNotes = schedule.Notes;
    }

    private async void SaveSchedule()
    {
        var title = EditorTitle.Trim();
        if (string.IsNullOrEmpty(title))
        {
            _toastService.Show("일정 제목을 입력해 주세요.", ToastType.Warning);
            return;
        }

        if (EditorStartTime is not { } startTime || EditorEndTime is not { } endTime || endTime <= startTime)
        {
            _toastService.Show("종료 시간은 시작 시간보다 늦어야 합니다.", ToastType.Warning);
            return;
        }

        if (IsEditing && SelectedSchedule is { } selected)
        {
            selected.Title = title;
            selected.Date = EditorDate;
            selected.StartTime = startTime;
            selected.EndTime = endTime;
            selected.Category = EditorCategory;
            selected.Notes = EditorNotes.Trim();
        }
        else
        {
            Schedules.Add(new ScheduleItem
            {
                Title = title,
                Date = EditorDate,
                StartTime = startTime,
                EndTime = endTime,
                Category = EditorCategory,
                Notes = EditorNotes.Trim()
            });
        }

        SelectedDate = EditorDate;
        DisplayDate = EditorDate;
        RefreshDailySchedules();
        await PersistAsync();
        BeginNewSchedule();
        _toastService.Show("일정이 저장되었습니다.", ToastType.Success);
    }

    private async void DeleteSchedule()
    {
        if (SelectedSchedule is not { } schedule)
        {
            return;
        }

        Schedules.Remove(schedule);
        RefreshDailySchedules();
        await PersistAsync();
        BeginNewSchedule();
        _toastService.Show("일정을 삭제했습니다.");
    }

    private async void ToggleCompleted(object? parameter)
    {
        if (parameter is ScheduleItem schedule)
        {
            schedule.IsCompleted = !schedule.IsCompleted;
            await PersistAsync();
        }
    }

    private async Task PersistAsync()
    {
        try
        {
            await _storageService.SaveAsync(Schedules);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "일정 데이터를 저장하지 못했습니다.");
            _toastService.Show("일정을 저장하지 못했습니다.", ToastType.Error);
        }
    }

    private static IReadOnlyList<ScheduleItem> CreateSampleSchedules() =>
    [
        new() { Title = "주간 업무 계획", Date = DateTime.Today, StartTime = new(9, 0, 0), EndTime = new(10, 0, 0), Category = "업무", Notes = "이번 주 우선순위를 정리합니다." },
        new() { Title = "프로젝트 미팅", Date = DateTime.Today, StartTime = new(14, 0, 0), EndTime = new(15, 0, 0), Category = "미팅", Notes = "진행 상황과 다음 마일스톤을 확인합니다." },
        new() { Title = "운동", Date = DateTime.Today.AddDays(1), StartTime = new(19, 0, 0), EndTime = new(20, 0, 0), Category = "개인" }
    ];
}
