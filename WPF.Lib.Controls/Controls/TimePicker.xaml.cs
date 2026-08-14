using System.Windows;
using System.Windows.Controls;

namespace WPF.Lib.Controls.Controls;

public partial class TimePicker : UserControl
{
    private bool _isUpdatingSelection;

    public static readonly DependencyProperty SelectedTimeProperty = DependencyProperty.Register(
        nameof(SelectedTime), typeof(TimeSpan?), typeof(TimePicker),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedTimeChanged, CoerceSelectedTime));

    public static readonly DependencyProperty MinuteStepProperty = DependencyProperty.Register(
        nameof(MinuteStep), typeof(int), typeof(TimePicker),
        new PropertyMetadata(5, OnMinuteStepChanged, CoerceMinuteStep));

    public static readonly DependencyProperty Is24HourModeProperty = DependencyProperty.Register(
        nameof(Is24HourMode), typeof(bool), typeof(TimePicker),
        new PropertyMetadata(true, OnIs24HourModeChanged));

    public TimePicker()
    {
        InitializeComponent();
        RebuildItems();
    }

    public TimeSpan? SelectedTime
    {
        get => (TimeSpan?)GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    public int MinuteStep
    {
        get => (int)GetValue(MinuteStepProperty);
        set => SetValue(MinuteStepProperty, value);
    }

    public bool Is24HourMode
    {
        get => (bool)GetValue(Is24HourModeProperty);
        set => SetValue(Is24HourModeProperty, value);
    }

    private void RebuildItems()
    {
        if (HourComboBox is null)
        {
            return;
        }

        _isUpdatingSelection = true;
        HourComboBox.ItemsSource = Is24HourMode
            ? Enumerable.Range(0, 24).Select(value => value.ToString("00")).ToArray()
            : Enumerable.Range(1, 12).Select(value => value.ToString("00")).ToArray();
        MinuteComboBox.ItemsSource = Enumerable.Range(0, 60)
            .Where(value => value % MinuteStep == 0)
            .Select(value => value.ToString("00"))
            .ToArray();
        PeriodComboBox.Visibility = Is24HourMode ? Visibility.Collapsed : Visibility.Visible;
        _isUpdatingSelection = false;
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        if (HourComboBox is null)
        {
            return;
        }

        _isUpdatingSelection = true;
        if (SelectedTime is not { } time)
        {
            HourComboBox.SelectedIndex = -1;
            MinuteComboBox.SelectedIndex = -1;
            PeriodComboBox.SelectedIndex = -1;
            _isUpdatingSelection = false;
            return;
        }

        var roundedMinute = Math.Min(59, (time.Minutes / MinuteStep) * MinuteStep);
        var displayedHour = Is24HourMode ? time.Hours : time.Hours % 12 == 0 ? 12 : time.Hours % 12;
        HourComboBox.SelectedItem = displayedHour.ToString("00");
        MinuteComboBox.SelectedItem = roundedMinute.ToString("00");
        PeriodComboBox.SelectedIndex = time.Hours >= 12 ? 1 : 0;
        _isUpdatingSelection = false;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingSelection || HourComboBox.SelectedItem is not string hourText ||
            MinuteComboBox.SelectedItem is not string minuteText ||
            !int.TryParse(hourText, out var hour) || !int.TryParse(minuteText, out var minute))
        {
            return;
        }

        if (!Is24HourMode)
        {
            hour %= 12;
            if (PeriodComboBox.SelectedIndex == 1)
            {
                hour += 12;
            }
        }

        SelectedTime = new TimeSpan(hour, minute, 0);
    }

    private static object CoerceSelectedTime(DependencyObject d, object baseValue)
    {
        if (baseValue is not TimeSpan time)
        {
            return baseValue;
        }

        return new TimeSpan(time.Hours, time.Minutes, 0);
    }

    private static object CoerceMinuteStep(DependencyObject d, object baseValue) =>
        Math.Clamp((int)baseValue, 1, 30);

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((TimePicker)d).UpdateSelection();

    private static void OnMinuteStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((TimePicker)d).RebuildItems();

    private static void OnIs24HourModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((TimePicker)d).RebuildItems();
}
