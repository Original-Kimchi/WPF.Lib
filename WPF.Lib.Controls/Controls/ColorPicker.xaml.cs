using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WPF.Lib.Controls.Controls;

public partial class ColorPicker : UserControl
{
    private bool _isUpdating;
    private bool _isDraggingColor;
    private bool _isDraggingHue;
    private double _hue;
    private double _saturation;
    private double _value;

    public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
        nameof(SelectedColor), typeof(Color), typeof(ColorPicker),
        new FrameworkPropertyMetadata(Colors.DodgerBlue,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
        nameof(IsDropDownOpen), typeof(bool), typeof(ColorPicker), new PropertyMetadata(false));

    public ColorPicker()
    {
        InitializeComponent();
        Loaded += (_, _) => UpdateSelection();
    }

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public SolidColorBrush CurrentHueBrush { get; } = new(Colors.Red);

    private static void OnSelectedColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) =>
        ((ColorPicker)sender).UpdateSelection();

    private void UpdateSelection()
    {
        if (HexInput is null)
        {
            return;
        }

        _isUpdating = true;
        try
        {
            var color = SelectedColor;
            RgbToHsv(color, out _hue, out _saturation, out _value);
            CurrentHueBrush.Color = HsvToColor(_hue, 1, 1, 255);
            RedInput.Value = color.R;
            GreenInput.Value = color.G;
            BlueInput.Value = color.B;
            AlphaInput.Value = color.A;
            var hex = FormatHex(color);
            HexInput.Text = hex;
            CollapsedHex.Text = hex;
            HexError.Visibility = Visibility.Collapsed;
            UpdateIndicators();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void UpdateIndicators()
    {
        if (ColorArea.ActualWidth <= 0 || ColorArea.ActualHeight <= 0 || HueArea.ActualHeight <= 0)
        {
            return;
        }

        Canvas.SetLeft(ColorIndicator, Math.Clamp((_saturation * ColorArea.ActualWidth) - (ColorIndicator.Width / 2),
            -ColorIndicator.Width / 2, ColorArea.ActualWidth - (ColorIndicator.Width / 2)));
        Canvas.SetTop(ColorIndicator, Math.Clamp(((1 - _value) * ColorArea.ActualHeight) - (ColorIndicator.Height / 2),
            -ColorIndicator.Height / 2, ColorArea.ActualHeight - (ColorIndicator.Height / 2)));
        Canvas.SetTop(HueIndicator, Math.Clamp((_hue / 360 * HueArea.ActualHeight) - (HueIndicator.Height / 2),
            -HueIndicator.Height / 2, HueArea.ActualHeight - (HueIndicator.Height / 2)));
    }

    private void SetColorFromHsv()
    {
        SetCurrentValue(SelectedColorProperty, HsvToColor(_hue, _saturation, _value, SelectedColor.A));
    }

    private void SelectColorAt(Point point)
    {
        _saturation = Math.Clamp(point.X / ColorArea.ActualWidth, 0, 1);
        _value = 1 - Math.Clamp(point.Y / ColorArea.ActualHeight, 0, 1);
        SetColorFromHsv();
    }

    private void SelectHueAt(Point point)
    {
        _hue = Math.Clamp(point.Y / HueArea.ActualHeight, 0, 1) * 360;
        CurrentHueBrush.Color = HsvToColor(_hue, 1, 1, 255);
        SetColorFromHsv();
    }

    private void OnColorAreaMouseDown(object sender, MouseButtonEventArgs e)
    {
        _isDraggingColor = true;
        ColorArea.CaptureMouse();
        SelectColorAt(e.GetPosition(ColorArea));
        e.Handled = true;
    }

    private void OnColorAreaMouseMove(object sender, MouseEventArgs e)
    {
        if (_isDraggingColor && e.LeftButton == MouseButtonState.Pressed)
        {
            SelectColorAt(e.GetPosition(ColorArea));
        }
    }

    private void OnHueMouseDown(object sender, MouseButtonEventArgs e)
    {
        _isDraggingHue = true;
        HueArea.CaptureMouse();
        SelectHueAt(e.GetPosition(HueArea));
        e.Handled = true;
    }

    private void OnHueMouseMove(object sender, MouseEventArgs e)
    {
        if (_isDraggingHue && e.LeftButton == MouseButtonState.Pressed)
        {
            SelectHueAt(e.GetPosition(HueArea));
        }
    }

    private void OnPickerMouseUp(object sender, MouseButtonEventArgs e)
    {
        _isDraggingColor = false;
        _isDraggingHue = false;
        Mouse.Capture(null);
    }

    private void OnRgbValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
    {
        if (_isUpdating || !IsInitialized)
        {
            return;
        }

        SetCurrentValue(SelectedColorProperty, Color.FromArgb(
            (byte)AlphaInput.Value, (byte)RedInput.Value, (byte)GreenInput.Value, (byte)BlueInput.Value));
    }

    private void CommitHex()
    {
        var text = HexInput.Text.Trim();
        if (text.StartsWith('#'))
        {
            text = text[1..];
        }

        if ((text.Length != 6 && text.Length != 8) ||
            !uint.TryParse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var value))
        {
            HexError.Visibility = Visibility.Visible;
            return;
        }

        var alpha = text.Length == 8 ? (byte)(value >> 24) : (byte)255;
        SetCurrentValue(SelectedColorProperty,
            Color.FromArgb(alpha, (byte)(value >> 16), (byte)(value >> 8), (byte)value));
    }

    private void OnHexKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitHex();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            UpdateSelection();
            e.Handled = true;
        }
    }

    private void OnHexLostFocus(object sender, KeyboardFocusChangedEventArgs e) => CommitHex();

    private void OnPopupOpened(object? sender, EventArgs e) =>
        Dispatcher.BeginInvoke(UpdateIndicators, DispatcherPriority.Loaded);

    private static string FormatHex(Color color) => color.A == 255
        ? $"#{color.R:X2}{color.G:X2}{color.B:X2}"
        : $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private static void RgbToHsv(Color color, out double hue, out double saturation, out double value)
    {
        var red = color.R / 255d;
        var green = color.G / 255d;
        var blue = color.B / 255d;
        var maximum = Math.Max(red, Math.Max(green, blue));
        var minimum = Math.Min(red, Math.Min(green, blue));
        var delta = maximum - minimum;
        hue = delta == 0 ? 0 : maximum == red
            ? 60 * (((green - blue) / delta) % 6)
            : maximum == green ? 60 * (((blue - red) / delta) + 2) : 60 * (((red - green) / delta) + 4);
        if (hue < 0)
        {
            hue += 360;
        }

        saturation = maximum == 0 ? 0 : delta / maximum;
        value = maximum;
    }

    private static Color HsvToColor(double hue, double saturation, double value, byte alpha)
    {
        hue = hue >= 360 ? 0 : hue;
        var chroma = value * saturation;
        var x = chroma * (1 - Math.Abs(((hue / 60) % 2) - 1));
        var offset = value - chroma;
        var (red, green, blue) = hue switch
        {
            < 60 => (chroma, x, 0d),
            < 120 => (x, chroma, 0d),
            < 180 => (0d, chroma, x),
            < 240 => (0d, x, chroma),
            < 300 => (x, 0d, chroma),
            _ => (chroma, 0d, x)
        };
        return Color.FromArgb(alpha,
            (byte)Math.Round((red + offset) * 255),
            (byte)Math.Round((green + offset) * 255),
            (byte)Math.Round((blue + offset) * 255));
    }
}
