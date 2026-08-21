using WPF.Lib.MVVM;

namespace WPFControls.UI.Models;

public sealed class ScheduleItem : BaseModel
{
    private string _title = string.Empty;
    private DateTime _date = DateTime.Today;
    private TimeSpan _startTime = new(9, 0, 0);
    private TimeSpan _endTime = new(10, 0, 0);
    private string _category = "업무";
    private string _notes = string.Empty;
    private bool _isCompleted;

    public Guid Id { get; init; } = Guid.NewGuid();

    public string Title { get => _title; set => SetProperty(ref _title, value); }
    public DateTime Date { get => _date; set => SetProperty(ref _date, value.Date); }

    public TimeSpan StartTime
    {
        get => _startTime;
        set
        {
            if (SetProperty(ref _startTime, value))
            {
                OnPropertyChanged(nameof(TimeRange));
            }
        }
    }

    public TimeSpan EndTime
    {
        get => _endTime;
        set
        {
            if (SetProperty(ref _endTime, value))
            {
                OnPropertyChanged(nameof(TimeRange));
            }
        }
    }

    public string Category { get => _category; set => SetProperty(ref _category, value); }
    public string Notes { get => _notes; set => SetProperty(ref _notes, value); }
    public bool IsCompleted { get => _isCompleted; set => SetProperty(ref _isCompleted, value); }
    public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
}
