using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WPF.Lib.Controls.Models;

namespace WPF.Lib.Controls.Converters;

public sealed class AnnotationKindToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var expectedKinds = parameter?.ToString()?.Split(',', StringSplitOptions.TrimEntries) ?? [];
        return value is ImageDrawingMode kind &&
               expectedKinds.Any(item => Enum.TryParse(item, out ImageDrawingMode expected) && kind == expected)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
