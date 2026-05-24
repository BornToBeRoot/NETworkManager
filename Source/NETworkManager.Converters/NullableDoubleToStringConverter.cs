using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;

/// <summary>
/// Converts a nullable <see cref="double"/> to a formatted string, returning "-/-" for null.
/// Pass a ConverterParameter of the form "F0|ms" or "F1|Mbps" to control the numeric format
/// specifier and the unit suffix, separated by '|'.
/// </summary>
public sealed class NullableDoubleToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double d)
            return "-/-";

        if (parameter is string fmt)
        {
            var parts = fmt.Split('|');
            var format = parts.Length > 0 ? parts[0] : "G";
            var unit = parts.Length > 1 ? " " + parts[1] : string.Empty;
            return d.ToString(format, culture) + unit;
        }

        return d.ToString(culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}