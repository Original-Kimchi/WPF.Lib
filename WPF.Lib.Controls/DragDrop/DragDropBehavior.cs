using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPF.Lib.Controls.DragDrop;

public static class DragDropBehavior
{
    private const string ItemDataFormat = "WPF.Lib.Controls.DragDrop.Item";

    public static readonly DependencyProperty IsDragSourceProperty = DependencyProperty.RegisterAttached(
        "IsDragSource",
        typeof(bool),
        typeof(DragDropBehavior),
        new PropertyMetadata(false, OnIsDragSourceChanged));

    public static readonly DependencyProperty IsDropTargetProperty = DependencyProperty.RegisterAttached(
        "IsDropTarget",
        typeof(bool),
        typeof(DragDropBehavior),
        new PropertyMetadata(false, OnIsDropTargetChanged));

    public static readonly DependencyProperty DropCommandProperty = DependencyProperty.RegisterAttached(
        "DropCommand",
        typeof(ICommand),
        typeof(DragDropBehavior));

    public static readonly DependencyProperty DragGroupProperty = DependencyProperty.RegisterAttached(
        "DragGroup",
        typeof(string),
        typeof(DragDropBehavior),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DropEffectProperty = DependencyProperty.RegisterAttached(
        "DropEffect",
        typeof(DragDropEffects),
        typeof(DragDropBehavior),
        new PropertyMetadata(DragDropEffects.Move));

    private static readonly DependencyProperty DragStartPointProperty = DependencyProperty.RegisterAttached(
        "DragStartPoint",
        typeof(Point),
        typeof(DragDropBehavior));

    private static readonly DependencyProperty DraggedItemProperty = DependencyProperty.RegisterAttached(
        "DraggedItem",
        typeof(object),
        typeof(DragDropBehavior));

    private static readonly DependencyPropertyKey IsDragOverPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "IsDragOver",
        typeof(bool),
        typeof(DragDropBehavior),
        new PropertyMetadata(false));

    public static readonly DependencyProperty IsDragOverProperty = IsDragOverPropertyKey.DependencyProperty;

    public static bool GetIsDragSource(DependencyObject element) => (bool)element.GetValue(IsDragSourceProperty);

    public static void SetIsDragSource(DependencyObject element, bool value) =>
        element.SetValue(IsDragSourceProperty, value);

    public static bool GetIsDropTarget(DependencyObject element) => (bool)element.GetValue(IsDropTargetProperty);

    public static void SetIsDropTarget(DependencyObject element, bool value) =>
        element.SetValue(IsDropTargetProperty, value);

    public static ICommand? GetDropCommand(DependencyObject element) =>
        (ICommand?)element.GetValue(DropCommandProperty);

    public static void SetDropCommand(DependencyObject element, ICommand? value) =>
        element.SetValue(DropCommandProperty, value);

    public static string GetDragGroup(DependencyObject element) =>
        (string)element.GetValue(DragGroupProperty);

    public static void SetDragGroup(DependencyObject element, string value) =>
        element.SetValue(DragGroupProperty, value);

    public static DragDropEffects GetDropEffect(DependencyObject element) =>
        (DragDropEffects)element.GetValue(DropEffectProperty);

    public static void SetDropEffect(DependencyObject element, DragDropEffects value) =>
        element.SetValue(DropEffectProperty, value);

    public static bool GetIsDragOver(DependencyObject element) =>
        (bool)element.GetValue(IsDragOverProperty);

    private static void OnIsDragSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        if ((bool)e.OldValue)
        {
            element.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            element.PreviewMouseMove -= OnPreviewMouseMove;
        }

        if ((bool)e.NewValue)
        {
            element.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            element.PreviewMouseMove += OnPreviewMouseMove;
        }
    }

    private static void OnIsDropTargetChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        if ((bool)e.OldValue)
        {
            element.Loaded -= OnDropTargetLoaded;
            element.Unloaded -= OnDropTargetUnloaded;
            DetachDropTarget(element);
        }

        if ((bool)e.NewValue)
        {
            element.Loaded += OnDropTargetLoaded;
            element.Unloaded += OnDropTargetUnloaded;
            if (element.IsLoaded)
            {
                AttachDropTarget(element);
            }
        }
    }

    private static void OnDropTargetLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && GetIsDropTarget(element))
        {
            AttachDropTarget(element);
        }
    }

    private static void OnDropTargetUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            DetachDropTarget(element);
        }
    }

    private static void AttachDropTarget(FrameworkElement element)
    {
        element.DragOver -= OnDragOver;
        element.DragLeave -= OnDragLeave;
        element.Drop -= OnDrop;
        element.DragOver += OnDragOver;
        element.DragLeave += OnDragLeave;
        element.Drop += OnDrop;
        element.AllowDrop = true;
    }

    private static void DetachDropTarget(FrameworkElement element)
    {
        element.DragOver -= OnDragOver;
        element.DragLeave -= OnDragLeave;
        element.Drop -= OnDrop;
        element.AllowDrop = false;
        element.SetValue(IsDragOverPropertyKey, false);
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        element.SetValue(DragStartPointProperty, e.GetPosition(element));
        element.SetValue(DraggedItemProperty, FindItem(element, e.OriginalSource as DependencyObject));
    }

    private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not FrameworkElement element || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var item = element.GetValue(DraggedItemProperty);
        if (item is null)
        {
            return;
        }

        var start = (Point)element.GetValue(DragStartPointProperty);
        var current = e.GetPosition(element);
        if (Math.Abs(current.X - start.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(current.Y - start.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        element.ClearValue(DraggedItemProperty);
        var payload = new ItemPayload(element, item, GetDragGroup(element));
        var data = new DataObject(ItemDataFormat, payload);
        System.Windows.DragDrop.DoDragDrop(element, data, DragDropEffects.Move | DragDropEffects.Copy);
    }

    private static void OnDragOver(object sender, DragEventArgs e)
    {
        if (sender is not FrameworkElement target)
        {
            return;
        }

        var info = CreateInfo(target, e);
        var command = GetDropCommand(target);
        var canDrop = info is not null && command?.CanExecute(info) == true;

        e.Effects = canDrop ? GetDropEffect(target) : DragDropEffects.None;
        target.SetValue(IsDragOverPropertyKey, canDrop);
        e.Handled = true;
    }

    private static void OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is FrameworkElement target)
        {
            target.SetValue(IsDragOverPropertyKey, false);
        }
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not FrameworkElement target)
        {
            return;
        }

        target.SetValue(IsDragOverPropertyKey, false);
        var info = CreateInfo(target, e);
        var command = GetDropCommand(target);
        if (info is not null && command?.CanExecute(info) == true)
        {
            command.Execute(info);
            e.Effects = GetDropEffect(target);
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private static DragDropInfo? CreateInfo(FrameworkElement target, DragEventArgs e)
    {
        var payload = e.Data.GetDataPresent(ItemDataFormat)
            ? e.Data.GetData(ItemDataFormat) as ItemPayload
            : null;
        var targetGroup = GetDragGroup(target);
        if (payload is not null &&
            !string.IsNullOrEmpty(targetGroup) &&
            !string.Equals(payload.Group, targetGroup, StringComparison.Ordinal))
        {
            return null;
        }

        var files = e.Data.GetDataPresent(DataFormats.FileDrop) &&
                    e.Data.GetData(DataFormats.FileDrop) is string[] droppedFiles
            ? droppedFiles
            : [];

        var (targetItem, insertionIndex, position) = GetDropLocation(target, e);
        return new DragDropInfo(
            payload?.Source ?? target,
            target,
            payload?.Item,
            targetItem,
            insertionIndex,
            position,
            files,
            e.Data);
    }

    private static (object? Item, int Index, DropPosition Position) GetDropLocation(
        FrameworkElement target,
        DragEventArgs e)
    {
        if (target is not ItemsControl itemsControl)
        {
            return (null, -1, DropPosition.None);
        }

        var container = ItemsControl.ContainerFromElement(itemsControl, e.OriginalSource as DependencyObject);
        if (container is not FrameworkElement containerElement)
        {
            return (null, itemsControl.Items.Count, DropPosition.After);
        }

        var item = itemsControl.ItemContainerGenerator.ItemFromContainer(containerElement);
        var index = itemsControl.ItemContainerGenerator.IndexFromContainer(containerElement);
        var point = e.GetPosition(containerElement);
        var position = point.Y < containerElement.ActualHeight / 2
            ? DropPosition.Before
            : DropPosition.After;

        return (item, position == DropPosition.After ? index + 1 : index, position);
    }

    private static object? FindItem(FrameworkElement element, DependencyObject? originalSource)
    {
        if (element is not ItemsControl itemsControl || originalSource is null)
        {
            return element.DataContext;
        }

        var container = ItemsControl.ContainerFromElement(itemsControl, originalSource);
        return container is null
            ? null
            : itemsControl.ItemContainerGenerator.ItemFromContainer(container);
    }

    private sealed record ItemPayload(FrameworkElement Source, object Item, string Group);
}
