using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;
/// <summary>
/// Convert a value of 0 to the boolean false.
/// </summary>
public class IntZeroToFalseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            null => false,
            int i => i is not 0,
            bool boolean => boolean ? 1 : 0,
            _ => false
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}