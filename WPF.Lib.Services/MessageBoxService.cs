using System.Windows;
using WPF.Lib.Controls.Controls;
using WPF.Lib.Controls.Models;
using WPF.Lib.Services.Abstractions;

namespace WPF.Lib.Services;

public sealed class MessageBoxService : IMessageBoxService
{
    public MessageBoxResult Show(
        string message,
        string title = "",
        MessageBoxButton buttons = MessageBoxButton.OK,
        MessageBoxType type = MessageBoxType.Default,
        MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        return ThemedMessageBox.Show(
            message,
            title,
            buttons,
            type,
            defaultResult);
    }
}
