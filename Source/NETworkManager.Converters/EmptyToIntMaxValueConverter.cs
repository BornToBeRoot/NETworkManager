using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;

public class EmptyToIntMaxValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        const int fallback = int.MaxValue; 
        if (targetType == typeof(int))
        {
            if (value is not string strValue)
                return fallback;
            if (string.IsNullOrWhiteSpace(strValue))
                return fallback;
            if (!int.TryParse(strValue, out int parsedIntValue))
                return fallback;
            return parsedIntValue;
        }

        if (targetType != typeof(string))
            return null;
        if (value is not int intValue)
            return null;
        return intValue is fallback ? string.Empty : intValue.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}