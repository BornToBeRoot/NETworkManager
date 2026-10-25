using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;

/// <summary>
///     Shows "..." while a value is pending (e.g. a probe of the IP Scanner is still running), otherwise the value.
///     Values: [0] pending (bool), [1] value (usually converted to a string by its own binding).
/// </summary>
public sealed class PendingToStringConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not { Length: 2 })
            return null;

        return values[0] is true ? "..." : values[1]?.ToString();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
