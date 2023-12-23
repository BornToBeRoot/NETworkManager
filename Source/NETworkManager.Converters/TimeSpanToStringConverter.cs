using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;

public sealed class TimeSpanToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not TimeSpan timeSpan
            ? "-/-"
            : $"{Math.Floor(timeSpan.TotalDays)}d {timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}