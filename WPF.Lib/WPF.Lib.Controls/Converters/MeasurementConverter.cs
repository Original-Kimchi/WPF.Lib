using System.Globalization;
using System.Windows.Data;
using WPF.Lib.Controls.Models;

namespace WPF.Lib.Controls.Converters;

public sealed class MeasurementConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 6 || values[0] is not ImageDrawingMode kind ||
            values[1] is not double width || values[2] is not double height ||
            values[3] is not double length || values[4] is not double unitsPerPixel ||
            values[5] is not string unit)
        {
            return string.Empty;
        }

        return kind == ImageDrawingMode.Line
            ? $"L {length * unitsPerPixel:0.##} {unit}"
            : $"W {width * unitsPerPixel:0.##} × H {height * unitsPerPixel:0.##} {unit}";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
