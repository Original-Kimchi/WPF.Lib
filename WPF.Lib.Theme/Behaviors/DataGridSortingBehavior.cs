using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace WPF.Lib.Theme.Behaviors;

/// <summary>
/// Adds an unsorted state to the standard <see cref="DataGrid"/> column sort cycle.
/// </summary>
public static class DataGridSortingBehavior
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(DataGridSortingBehavior),
        new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject element) =>
        (bool)element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid dataGrid)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            dataGrid.Sorting += OnSorting;
        }
        else
        {
            dataGrid.Sorting -= OnSorting;
        }
    }

    private static void OnSorting(object sender, DataGridSortingEventArgs e)
    {
        if (sender is not DataGrid dataGrid ||
            e.Column.SortDirection != ListSortDirection.Descending)
        {
            return;
        }

        e.Handled = true;
        dataGrid.Items.SortDescriptions.Clear();

        foreach (var column in dataGrid.Columns)
        {
            column.SortDirection = null;
        }
    }
}
