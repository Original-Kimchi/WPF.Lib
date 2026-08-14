using System.Windows;
using System.Windows.Controls;

namespace WPF.Lib.Controls.Controls;

public partial class BusyOverlay : UserControl
{
    public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(
        nameof(Child), typeof(object), typeof(BusyOverlay));

    public static readonly DependencyProperty ChildTemplateProperty = DependencyProperty.Register(
        nameof(ChildTemplate), typeof(DataTemplate), typeof(BusyOverlay));

    public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
        nameof(IsBusy), typeof(bool), typeof(BusyOverlay), new PropertyMetadata(false));

    public static readonly DependencyProperty BusyMessageProperty = DependencyProperty.Register(
        nameof(BusyMessage), typeof(string), typeof(BusyOverlay), new PropertyMetadata("Working..."));

    public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register(
        nameof(IsIndeterminate), typeof(bool), typeof(BusyOverlay), new PropertyMetadata(true));

    public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
        nameof(Progress), typeof(double), typeof(BusyOverlay), new PropertyMetadata(0d, null, CoerceProgress));

    public BusyOverlay() => InitializeComponent();

    public object? Child
    {
        get => GetValue(ChildProperty);
        set => SetValue(ChildProperty, value);
    }

    public DataTemplate? ChildTemplate
    {
        get => (DataTemplate?)GetValue(ChildTemplateProperty);
        set => SetValue(ChildTemplateProperty, value);
    }

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public string BusyMessage
    {
        get => (string)GetValue(BusyMessageProperty);
        set => SetValue(BusyMessageProperty, value);
    }

    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    private static object CoerceProgress(DependencyObject d, object baseValue) =>
        Math.Clamp((double)baseValue, 0d, 100d);
}
