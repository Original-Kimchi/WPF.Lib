using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WPF.Lib.Controls.Behaviors;

/// <summary>
/// Forwards the mouse wheel to a scrollable ancestor when the current
/// <see cref="ScrollViewer"/> has reached its vertical boundary.
/// </summary>
public static class NestedScrollBehavior
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(NestedScrollBehavior),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    private static bool _isRegistered;

    public static bool GetIsEnabled(DependencyObject element) =>
        (bool)element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    internal static void Register()
    {
        if (_isRegistered)
        {
            return;
        }

        EventManager.RegisterClassHandler(
            typeof(ScrollViewer),
            UIElement.PreviewMouseWheelEvent,
            new MouseWheelEventHandler(OnPreviewMouseWheel));

        _isRegistered = true;
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled ||
            sender is not ScrollViewer current ||
            !GetIsEnabled(current))
        {
            return;
        }

        var modifiers = Keyboard.Modifiers;
        var scrollHorizontally = modifiers.HasFlag(ModifierKeys.Shift) &&
                                 !modifiers.HasFlag(ModifierKeys.Control);

        if (scrollHorizontally)
        {
            var horizontalScrollViewer = FindNearestHorizontalScrollViewer(
                e.OriginalSource as DependencyObject);

            if (!ReferenceEquals(current, horizontalScrollViewer))
            {
                return;
            }

            var horizontalTarget = CanScrollHorizontally(current, e.Delta)
                ? current
                : FindScrollableParent(current, e.Delta, horizontal: true);

            // Do not fall back to vertical scrolling when a horizontal region
            // exists but has reached its boundary.
            e.Handled = true;

            if (horizontalTarget is not null)
            {
                ScrollHorizontally(horizontalTarget, e.Delta);
            }

            return;
        }

        if (CanScrollVertically(current, e.Delta))
        {
            return;
        }

        var parent = FindScrollableParent(current, e.Delta, horizontal: false);
        if (parent is null)
        {
            return;
        }

        e.Handled = true;

        parent.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = UIElement.MouseWheelEvent,
            Source = current
        });
    }

    private static bool CanScrollVertically(ScrollViewer scrollViewer, int delta)
    {
        if (scrollViewer.ScrollableHeight <= 0)
        {
            return false;
        }

        return delta switch
        {
            > 0 => scrollViewer.VerticalOffset > 0,
            < 0 => scrollViewer.VerticalOffset < scrollViewer.ScrollableHeight,
            _ => false
        };
    }

    private static bool CanScrollHorizontally(ScrollViewer scrollViewer, int delta)
    {
        if (scrollViewer.ScrollableWidth <= 0)
        {
            return false;
        }

        return delta switch
        {
            > 0 => scrollViewer.HorizontalOffset > 0,
            < 0 => scrollViewer.HorizontalOffset < scrollViewer.ScrollableWidth,
            _ => false
        };
    }

    private static ScrollViewer? FindNearestHorizontalScrollViewer(DependencyObject? child)
    {
        var current = child;

        while (current is not null)
        {
            if (current is ScrollViewer scrollViewer &&
                GetIsEnabled(scrollViewer) &&
                scrollViewer.ScrollableWidth > 0)
            {
                return scrollViewer;
            }

            current = GetParent(current);
        }

        return null;
    }

    private static ScrollViewer? FindScrollableParent(
        DependencyObject child,
        int delta,
        bool horizontal)
    {
        var current = GetParent(child);

        while (current is not null)
        {
            if (current is ScrollViewer scrollViewer &&
                GetIsEnabled(scrollViewer) &&
                (horizontal
                    ? CanScrollHorizontally(scrollViewer, delta)
                    : CanScrollVertically(scrollViewer, delta)))
            {
                return scrollViewer;
            }

            current = GetParent(current);
        }

        return null;
    }

    private static void ScrollHorizontally(ScrollViewer scrollViewer, int delta)
    {
        var wheelNotches = Math.Max(
            1,
            (int)Math.Ceiling(Math.Abs(delta) / (double)Mouse.MouseWheelDeltaForOneLine));

        if (SystemParameters.WheelScrollLines == -1)
        {
            for (var index = 0; index < wheelNotches; index++)
            {
                if (delta > 0)
                {
                    scrollViewer.PageLeft();
                }
                else
                {
                    scrollViewer.PageRight();
                }
            }

            return;
        }

        var lineCount = SystemParameters.WheelScrollLines * wheelNotches;

        for (var index = 0; index < lineCount; index++)
        {
            if (delta > 0)
            {
                scrollViewer.LineLeft();
            }
            else
            {
                scrollViewer.LineRight();
            }
        }
    }

    private static DependencyObject? GetParent(DependencyObject child) => child switch
    {
        Visual or Visual3D => VisualTreeHelper.GetParent(child),
        FrameworkContentElement contentElement => contentElement.Parent,
        ContentElement contentElement => ContentOperations.GetParent(contentElement),
        _ => null
    };
}
