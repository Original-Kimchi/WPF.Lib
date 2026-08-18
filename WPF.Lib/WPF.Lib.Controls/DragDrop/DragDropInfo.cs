using System.Windows;
using System.Windows.Controls;

namespace WPF.Lib.Controls.DragDrop;

public sealed record DragDropInfo(
    FrameworkElement Source,
    FrameworkElement Target,
    object? DraggedItem,
    object? TargetItem,
    int InsertionIndex,
    DropPosition Position,
    IReadOnlyList<string> Files,
    IDataObject Data)
{
    public bool HasFiles => Files.Count > 0;

    public ItemsControl? SourceItemsControl => Source as ItemsControl;

    public ItemsControl? TargetItemsControl => Target as ItemsControl;
}
