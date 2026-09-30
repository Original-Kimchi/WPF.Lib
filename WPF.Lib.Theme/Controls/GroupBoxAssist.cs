using System.Windows;
using System.Windows.Media;

namespace WPF.Lib.Theme.Controls;

public static class GroupBoxAssist
{
    public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.RegisterAttached(
        "HeaderBackground",
        typeof(Brush),
        typeof(GroupBoxAssist),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty HeaderForegroundProperty = DependencyProperty.RegisterAttached(
        "HeaderForeground",
        typeof(Brush),
        typeof(GroupBoxAssist),
        new FrameworkPropertyMetadata(null));

    public static void SetHeaderBackground(DependencyObject element, Brush? value) =>
        element.SetValue(HeaderBackgroundProperty, value);

    public static Brush? GetHeaderBackground(DependencyObject element) =>
        (Brush?)element.GetValue(HeaderBackgroundProperty);

    public static void SetHeaderForeground(DependencyObject element, Brush? value) =>
        element.SetValue(HeaderForegroundProperty, value);

    public static Brush? GetHeaderForeground(DependencyObject element) =>
        (Brush?)element.GetValue(HeaderForegroundProperty);
}
