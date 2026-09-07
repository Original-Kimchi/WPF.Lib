using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPF.Lib.Controls.Controls;

public partial class ProgressRing : UserControl
{
    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
        nameof(IsActive), typeof(bool), typeof(ProgressRing), new PropertyMetadata(true));

    public static readonly DependencyProperty RingBrushProperty = DependencyProperty.Register(
        nameof(RingBrush), typeof(Brush), typeof(ProgressRing));

    public static readonly DependencyProperty RingThicknessProperty = DependencyProperty.Register(
        nameof(RingThickness), typeof(double), typeof(ProgressRing),
        new PropertyMetadata(3d, null, CoerceRingThickness));

    public ProgressRing()
    {
        InitializeComponent();
        SetResourceReference(RingBrushProperty, "PrimaryBrush");
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public Brush? RingBrush
    {
        get => (Brush?)GetValue(RingBrushProperty);
        set => SetValue(RingBrushProperty, value);
    }

    public double RingThickness
    {
        get => (double)GetValue(RingThicknessProperty);
        set => SetValue(RingThicknessProperty, value);
    }

    private static object CoerceRingThickness(DependencyObject d, object baseValue) =>
        Math.Max(0.5d, (double)baseValue);
}
