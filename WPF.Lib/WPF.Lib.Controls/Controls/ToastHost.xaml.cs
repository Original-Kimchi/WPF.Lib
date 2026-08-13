using System.Windows;
using System.Windows.Controls;
using WPF.Lib.Controls.Abstractions;
using WPF.Lib.Controls.Models;

namespace WPF.Lib.Controls.Controls;

public partial class ToastHost : UserControl
{
    public static readonly DependencyProperty ServiceProperty = DependencyProperty.Register(
        nameof(Service), typeof(IToastService), typeof(ToastHost));

    public ToastHost() => InitializeComponent();

    public IToastService? Service
    {
        get => (IToastService?)GetValue(ServiceProperty);
        set => SetValue(ServiceProperty, value);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ToastNotification notification })
        {
            Service?.Dismiss(notification);
        }
    }
}
