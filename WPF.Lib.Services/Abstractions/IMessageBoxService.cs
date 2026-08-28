using System.Windows;
using WPF.Lib.Controls.Models;

namespace WPF.Lib.Services.Abstractions;

public interface IMessageBoxService
{
    MessageBoxResult Show(
        string message,
        string title = "",
        MessageBoxButton buttons = MessageBoxButton.OK,
        MessageBoxType type = MessageBoxType.Default,
        MessageBoxResult defaultResult = MessageBoxResult.None);
}
