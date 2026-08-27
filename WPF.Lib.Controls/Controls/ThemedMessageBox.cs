using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using WPF.Lib.Controls.Models;

namespace WPF.Lib.Controls.Controls;

public static class ThemedMessageBox
{
    public static MessageBoxResult Show(string messageBoxText) =>
        Show(messageBoxText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None);

    public static MessageBoxResult Show(string messageBoxText, string caption) =>
        Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);

    public static MessageBoxResult Show(
        string messageBoxText,
        string caption,
        MessageBoxButton button) =>
        Show(messageBoxText, caption, button, MessageBoxImage.None);

    public static MessageBoxResult Show(
        string messageBoxText,
        string caption,
        MessageBoxButton button,
        MessageBoxImage icon) =>
        Show(messageBoxText, caption, button, icon, MessageBoxResult.None);

    public static MessageBoxResult Show(
        string messageBoxText,
        string caption,
        MessageBoxButton button,
        MessageBoxImage icon,
        MessageBoxResult defaultResult) =>
        ShowCore(FindOwner(), messageBoxText, caption, button, icon, MessageBoxType.Default, defaultResult);

    public static MessageBoxResult Show(
        string messageBoxText,
        string caption,
        MessageBoxButton button,
        MessageBoxType type,
        MessageBoxResult defaultResult = MessageBoxResult.None) =>
        ShowCore(FindOwner(), messageBoxText, caption, button, MessageBoxImage.None, type, defaultResult);

    public static MessageBoxResult Show(Window owner, string messageBoxText) =>
        Show(owner, messageBoxText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None);

    public static MessageBoxResult Show(Window owner, string messageBoxText, string caption) =>
        Show(owner, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);

    public static MessageBoxResult Show(
        Window owner,
        string messageBoxText,
        string caption,
        MessageBoxButton button) =>
        Show(owner, messageBoxText, caption, button, MessageBoxImage.None);

    public static MessageBoxResult Show(
        Window owner,
        string messageBoxText,
        string caption,
        MessageBoxButton button,
        MessageBoxImage icon) =>
        Show(owner, messageBoxText, caption, button, icon, MessageBoxResult.None);

    public static MessageBoxResult Show(
        Window? owner,
        string messageBoxText,
        string caption,
        MessageBoxButton button,
        MessageBoxImage icon,
        MessageBoxResult defaultResult)
        => ShowCore(owner, messageBoxText, caption, button, icon, MessageBoxType.Default, defaultResult);

    public static MessageBoxResult Show(
        Window? owner,
        string messageBoxText,
        string caption,
        MessageBoxButton button,
        MessageBoxType type,
        MessageBoxResult defaultResult = MessageBoxResult.None) =>
        ShowCore(owner, messageBoxText, caption, button, MessageBoxImage.None, type, defaultResult);

    private static MessageBoxResult ShowCore(
        Window? owner,
        string messageBoxText,
        string caption,
        MessageBoxButton button,
        MessageBoxImage icon,
        MessageBoxType type,
        MessageBoxResult defaultResult)
    {
        ArgumentNullException.ThrowIfNull(messageBoxText);
        ArgumentNullException.ThrowIfNull(caption);
        Validate(button, icon, type, defaultResult);

        var dispatcher = owner?.Dispatcher ?? Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        if (!dispatcher.CheckAccess())
        {
            return dispatcher.Invoke(() =>
                ShowCore(owner, messageBoxText, caption, button, icon, type, defaultResult));
        }

        var dialog = new ThemedMessageBoxWindow(messageBoxText, caption, button, icon, type, defaultResult)
        {
            Owner = owner,
            WindowStartupLocation = owner is null
                ? WindowStartupLocation.CenterScreen
                : WindowStartupLocation.CenterOwner
        };

        dialog.ShowDialog();
        return dialog.Result;
    }

    private static Window? FindOwner() =>
        Application.Current?.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
        ?? Application.Current?.MainWindow;

    private static void Validate(
        MessageBoxButton button,
        MessageBoxImage icon,
        MessageBoxType type,
        MessageBoxResult defaultResult)
    {
        if (!Enum.IsDefined(button))
        {
            throw new InvalidEnumArgumentException(nameof(button), (int)button, typeof(MessageBoxButton));
        }

        if (!Enum.IsDefined(icon))
        {
            throw new InvalidEnumArgumentException(nameof(icon), (int)icon, typeof(MessageBoxImage));
        }

        if (!Enum.IsDefined(type))
        {
            throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(MessageBoxType));
        }

        if (!Enum.IsDefined(defaultResult))
        {
            throw new InvalidEnumArgumentException(
                nameof(defaultResult),
                (int)defaultResult,
                typeof(MessageBoxResult));
        }

        var isValidDefault = defaultResult == MessageBoxResult.None || button switch
        {
            MessageBoxButton.OK => defaultResult == MessageBoxResult.OK,
            MessageBoxButton.OKCancel => defaultResult is MessageBoxResult.OK or MessageBoxResult.Cancel,
            MessageBoxButton.YesNo => defaultResult is MessageBoxResult.Yes or MessageBoxResult.No,
            MessageBoxButton.YesNoCancel => defaultResult is MessageBoxResult.Yes or MessageBoxResult.No or MessageBoxResult.Cancel,
            _ => false
        };

        if (!isValidDefault)
        {
            throw new ArgumentException("The default result must match one of the configured buttons.", nameof(defaultResult));
        }
    }
}
