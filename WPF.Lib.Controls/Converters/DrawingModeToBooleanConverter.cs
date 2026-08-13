using System.Globalization;
using System.Windows.Data;
using WPF.Lib.Controls.Models;

namespace WPF.Lib.Controls.Converters;

public sealed class DrawingModeToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is ImageDrawingMode mode &&
               Enum.TryParse(parameter?.ToString(), out ImageDrawingMode expected) &&
               mode == expected;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
