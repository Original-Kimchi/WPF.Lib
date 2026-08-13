using System.Globalization;
using System.Windows.Data;

namespace WPF.Lib.Controls.Converters;

public sealed class InverseScaleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is double scale && scale > 0 ? 1 / scale : 1d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
