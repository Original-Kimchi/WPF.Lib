using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPF.Lib.Theme.Behaviors;

/// <summary>
/// Toggles a tree view item's expanded state when its row is clicked.
/// </summary>
public static class TreeViewItemExpandBehavior
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(TreeViewItemExpandBehavior),
        new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject element) =>
        (bool)element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TreeView treeView)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            treeView.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            treeView.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }
        else
        {
            treeView.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            treeView.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        }
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount > 1 && TryGetExpandableItem(e, out _))
        {
            // TreeViewItem toggles IsExpanded on a double-click by default. Suppress
            // that second toggle because this behavior already handled the first click.
            e.Handled = true;
        }
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount > 1 || !TryGetExpandableItem(e, out var item))
        {
            return;
        }

        item.IsExpanded = !item.IsExpanded;
        e.Handled = true;
    }

    private static bool TryGetExpandableItem(MouseEventArgs e, out TreeViewItem item)
    {
        if (e.OriginalSource is DependencyObject source &&
            FindParent<TreeViewItem>(source) is { HasItems: true } expandableItem)
        {
            item = expandableItem;
            return true;
        }

        item = null!;
        return false;
    }

    private static T? FindParent<T>(DependencyObject source)
        where T : DependencyObject
    {
        DependencyObject? current = source;

        while (current is not null)
        {
            if (current is T match)
            {
                return match;
            }

            current = current switch
            {
                Visual => VisualTreeHelper.GetParent(current),
                FrameworkContentElement contentElement => contentElement.Parent,
                _ => LogicalTreeHelper.GetParent(current),
            };
        }

        return null;
    }
}
