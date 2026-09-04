using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPF.Lib.Controls.Controls;

public partial class ColorPicker : UserControl
{
    private bool _isUpdating;

    public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
        nameof(SelectedColor), typeof(Color), typeof(ColorPicker),
        new FrameworkPropertyMetadata(Colors.DodgerBlue,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

    public ColorPicker()
    {
        InitializeComponent();
        Palette.ItemsSource = new[]
        {
            Colors.Black, Colors.DimGray, Colors.Silver, Colors.White,
            Colors.Firebrick, Colors.OrangeRed, Colors.Orange, Colors.Gold,
            Colors.ForestGreen, Colors.LimeGreen, Colors.Teal, Colors.Turquoise,
            Colors.DodgerBlue, Colors.RoyalBlue, Colors.BlueViolet, Colors.DeepPink
        };
        UpdateSelection();
    }

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    private static void OnSelectedColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        ((ColorPicker)sender).UpdateSelection();
    }

    private void UpdateSelection()
    {
        if (AlphaSlider is null || OpacityLabel is null)
        {
            return;
        }

        _isUpdating = true;
        try
        {
            var color = SelectedColor;
            HexInput.Text = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            HexError.Visibility = Visibility.Collapsed;
            AlphaSlider.Value = color.A;
            OpacityLabel.Text = $"{color.A / 255d:P0}";
            Palette.SelectedItem = Color.FromRgb(color.R, color.G, color.B);
        }
        finally
        {
            _isUpdating = false;
        }
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
        SetCurrentValue(SelectedColorProperty, Color.FromArgb(alpha, (byte)(value >> 16), (byte)(value >> 8), (byte)value));
        UpdateSelection();
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

    private void OnPaletteSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isUpdating && Palette.SelectedItem is Color color)
        {
            SetCurrentValue(SelectedColorProperty, Color.FromArgb(SelectedColor.A, color.R, color.G, color.B));
        }
    }

    private void OnAlphaChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_isUpdating && IsInitialized)
        {
            var color = SelectedColor;
            SetCurrentValue(SelectedColorProperty, Color.FromArgb((byte)Math.Round(e.NewValue), color.R, color.G, color.B));
        }
    }
}
