using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPF.Lib.Controls.Models;

namespace WPF.Lib.Controls.Controls;

public partial class ThemedMessageBoxWindow : Window
{
    private readonly MessageBoxButton _buttons;
    private readonly MessageBoxResult _closeResult;
    private bool _escapeCloseRequested;
    private bool _resultSelected;

    internal ThemedMessageBoxWindow(
        string message,
        string caption,
        MessageBoxButton buttons,
        MessageBoxImage icon,
        MessageBoxType type,
        MessageBoxResult defaultResult)
    {
        InitializeComponent();

        _buttons = buttons;
        _closeResult = GetCloseResult(buttons);
        Title = caption;
        MessageTextBlock.Text = message;

        ConfigureButtons(buttons, defaultResult);
        ConfigureAppearance(icon, type);

        Loaded += OnLoaded;
        PreviewKeyDown += OnPreviewKeyDown;
    }

    public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

    private void ConfigureButtons(MessageBoxButton buttons, MessageBoxResult defaultResult)
    {
        OkButton.Visibility = buttons is MessageBoxButton.OK or MessageBoxButton.OKCancel
            ? Visibility.Visible
            : Visibility.Collapsed;
        YesButton.Visibility = buttons is MessageBoxButton.YesNo or MessageBoxButton.YesNoCancel
            ? Visibility.Visible
            : Visibility.Collapsed;
        NoButton.Visibility = buttons is MessageBoxButton.YesNo or MessageBoxButton.YesNoCancel
            ? Visibility.Visible
            : Visibility.Collapsed;
        CancelButton.Visibility = buttons is MessageBoxButton.OKCancel or MessageBoxButton.YesNoCancel
            ? Visibility.Visible
            : Visibility.Collapsed;

        foreach (var button in GetVisibleButtons())
        {
            var buttonResult = ParseResult(button.Tag);
            button.IsDefault = buttonResult == defaultResult ||
                               defaultResult == MessageBoxResult.None && button == GetVisibleButtons().First();
        }

        CloseButton.IsEnabled = buttons != MessageBoxButton.YesNo;
    }

    private void ConfigureAppearance(MessageBoxImage icon, MessageBoxType type)
    {
        var (text, brushKey, headerBackgroundBrushKey, useDarkForeground) = type switch
        {
            MessageBoxType.Information =>
                ("i", "InfoBrush", "MessageBoxInfoHeaderBackgroundBrush", false),
            MessageBoxType.Success =>
                ("✓", "SuccessBrush", "MessageBoxSuccessHeaderBackgroundBrush", false),
            MessageBoxType.Warning =>
                ("!", "WarningBrush", "MessageBoxWarningHeaderBackgroundBrush", true),
            MessageBoxType.Error =>
                ("×", "DangerBrush", "MessageBoxErrorHeaderBackgroundBrush", false),
            _ => GetIconAppearance(icon)
        };

        if (string.IsNullOrEmpty(text))
        {
            IconBackground.Visibility = Visibility.Collapsed;
            MessageScrollViewer.SetValue(Grid.ColumnProperty, 0);
            MessageScrollViewer.SetValue(Grid.ColumnSpanProperty, 2);
            return;
        }

        IconText.Text = text;
        IconText.Foreground = useDarkForeground
            ? new SolidColorBrush(Color.FromRgb(17, 24, 39))
            : Brushes.White;
        IconBackground.SetResourceReference(Border.BackgroundProperty, brushKey);
        HeaderBorder.SetResourceReference(Border.BackgroundProperty, headerBackgroundBrushKey);
        HeaderTitle.SetResourceReference(TextBlock.ForegroundProperty, brushKey);
    }

    private static (string Text, string BrushKey, string HeaderBackgroundBrushKey, bool UseDarkForeground)
        GetIconAppearance(
        MessageBoxImage icon) => (int)icon switch
        {
            16 => ("×", "DangerBrush", "MessageBoxErrorHeaderBackgroundBrush", false),
            32 => ("?", "PrimaryBrush", "MessageBoxInfoHeaderBackgroundBrush", false),
            48 => ("!", "WarningBrush", "MessageBoxWarningHeaderBackgroundBrush", true),
            64 => ("i", "InfoBrush", "MessageBoxInfoHeaderBackgroundBrush", false),
            _ => (string.Empty, string.Empty, string.Empty, false)
        };

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        GetVisibleButtons().First(button => button.IsDefault).Focus();
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _escapeCloseRequested = true;
            Close();
            e.Handled = true;
            return;
        }

        if (Keyboard.Modifiers == ModifierKeys.None &&
            e.Key is Key.Left or Key.Right &&
            MoveButtonFocus(e.Key))
        {
            e.Handled = true;
            return;
        }

        if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
        {
            CopyMessageToClipboard();
            e.Handled = true;
        }
    }

    private bool MoveButtonFocus(Key key)
    {
        var buttons = GetVisibleButtons().ToList();
        if (buttons.Count < 2 || Keyboard.FocusedElement is not Button focusedButton)
        {
            return false;
        }

        var currentIndex = buttons.IndexOf(focusedButton);
        if (currentIndex < 0)
        {
            return false;
        }

        var offset = key == Key.Left ? -1 : 1;
        var nextIndex = (currentIndex + offset + buttons.Count) % buttons.Count;
        return buttons[nextIndex].Focus();
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void ResultButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        Result = ParseResult(button.Tag);
        _resultSelected = true;
        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_resultSelected && _buttons == MessageBoxButton.YesNo && !_escapeCloseRequested)
        {
            e.Cancel = true;
            return;
        }

        if (!_resultSelected)
        {
            Result = _closeResult;
        }

        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        Loaded -= OnLoaded;
        PreviewKeyDown -= OnPreviewKeyDown;
        base.OnClosed(e);
    }

    private IEnumerable<Button> GetVisibleButtons()
    {
        Button[] buttons = [OkButton, YesButton, NoButton, CancelButton];
        return buttons.Where(button => button.Visibility == Visibility.Visible);
    }

    private void CopyMessageToClipboard()
    {
        var separator = new string('-', 32);
        var text = new StringBuilder()
            .AppendLine(separator)
            .AppendLine(Title)
            .AppendLine(separator)
            .AppendLine(MessageTextBlock.Text)
            .AppendLine(separator)
            .AppendJoin("   ", GetVisibleButtons().Select(button => button.Content?.ToString()))
            .AppendLine()
            .AppendLine(separator)
            .ToString();

        try
        {
            Clipboard.SetText(text);
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            // The clipboard can be temporarily locked by another process.
        }
    }

    private static MessageBoxResult ParseResult(object? value) =>
        Enum.TryParse(value?.ToString(), out MessageBoxResult result)
            ? result
            : MessageBoxResult.None;

    private static MessageBoxResult GetCloseResult(MessageBoxButton buttons) => buttons switch
    {
        MessageBoxButton.OK => MessageBoxResult.OK,
        MessageBoxButton.OKCancel or MessageBoxButton.YesNoCancel => MessageBoxResult.Cancel,
        _ => MessageBoxResult.None
    };
}
