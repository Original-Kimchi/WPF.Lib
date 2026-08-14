using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPF.Lib.Controls.Controls;

public partial class NumericUpDown : UserControl
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(decimal), typeof(NumericUpDown),
        new FrameworkPropertyMetadata(0m, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue));

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum), typeof(decimal), typeof(NumericUpDown),
        new FrameworkPropertyMetadata(decimal.MinValue, OnRangeChanged, CoerceMinimum));

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum), typeof(decimal), typeof(NumericUpDown),
        new FrameworkPropertyMetadata(decimal.MaxValue, OnRangeChanged, CoerceMaximum));

    public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
        nameof(Increment), typeof(decimal), typeof(NumericUpDown),
        new FrameworkPropertyMetadata(1m, null, CoerceIncrement));

    public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register(
        nameof(DecimalPlaces), typeof(int), typeof(NumericUpDown),
        new PropertyMetadata(0, OnDecimalPlacesChanged, CoerceDecimalPlaces));

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly), typeof(bool), typeof(NumericUpDown), new PropertyMetadata(false, OnIsReadOnlyChanged));

    public NumericUpDown()
    {
        InitializeComponent();
        UpdateText();
        PreviewMouseWheel += OnPreviewMouseWheel;
        GotKeyboardFocus += (_, _) => OuterBorder.SetResourceReference(Border.BorderBrushProperty, "PrimaryBrush");
        LostKeyboardFocus += (_, _) => OuterBorder.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");
    }

    public decimal Value
    {
        get => (decimal)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public decimal Minimum
    {
        get => (decimal)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public decimal Maximum
    {
        get => (decimal)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public decimal Increment
    {
        get => (decimal)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    private void ChangeValue(decimal direction)
    {
        if (!IsEnabled || IsReadOnly)
        {
            return;
        }

        Value = Math.Clamp(Value + (Increment * direction), Minimum, Maximum);
    }

    private void CommitText()
    {
        if (decimal.TryParse(ValueTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var value))
        {
            Value = Math.Clamp(value, Minimum, Maximum);
        }

        UpdateText();
    }

    private void UpdateText()
    {
        if (ValueTextBox is not null)
        {
            ValueTextBox.Text = Value.ToString($"N{DecimalPlaces}", CultureInfo.CurrentCulture);
        }
    }

    private void OnIncreaseClick(object sender, RoutedEventArgs e) => ChangeValue(1m);

    private void OnDecreaseClick(object sender, RoutedEventArgs e) => ChangeValue(-1m);

    private void OnTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => CommitText();

    private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitText();
            e.Handled = true;
        }
        else if (e.Key == Key.Up)
        {
            ChangeValue(1m);
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            ChangeValue(-1m);
            e.Handled = true;
        }
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!IsKeyboardFocusWithin)
        {
            return;
        }

        ChangeValue(e.Delta > 0 ? 1m : -1m);
        e.Handled = true;
    }

    private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) => UpdateButtonState();

    private void UpdateButtonState()
    {
        if (IncreaseButton is null || DecreaseButton is null)
        {
            return;
        }

        IncreaseButton.IsEnabled = IsEnabled && !IsReadOnly && Value < Maximum;
        DecreaseButton.IsEnabled = IsEnabled && !IsReadOnly && Value > Minimum;
        Opacity = IsEnabled ? 1 : 0.55;
    }

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        var control = (NumericUpDown)d;
        return Math.Clamp((decimal)baseValue, control.Minimum, control.Maximum);
    }

    private static object CoerceMinimum(DependencyObject d, object baseValue) =>
        Math.Min((decimal)baseValue, ((NumericUpDown)d).Maximum);

    private static object CoerceMaximum(DependencyObject d, object baseValue) =>
        Math.Max((decimal)baseValue, ((NumericUpDown)d).Minimum);

    private static object CoerceIncrement(DependencyObject d, object baseValue)
    {
        var increment = (decimal)baseValue;
        if (increment == decimal.MinValue)
        {
            return decimal.MaxValue;
        }

        return increment == 0 ? 1m : Math.Abs(increment);
    }

    private static object CoerceDecimalPlaces(DependencyObject d, object baseValue) =>
        Math.Clamp((int)baseValue, 0, 28);

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (NumericUpDown)d;
        control.UpdateText();
        control.UpdateButtonState();
    }

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (NumericUpDown)d;
        control.CoerceValue(ValueProperty);
        control.UpdateButtonState();
    }

    private static void OnDecimalPlacesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((NumericUpDown)d).UpdateText();

    private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((NumericUpDown)d).UpdateButtonState();
}
