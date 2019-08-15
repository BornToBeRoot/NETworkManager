using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class TimeSpanToMillisecondConverter : IValueConverter
    {
        /* Translate the name of the accent */
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TimeSpan timeSpan))
                return "-/-";


            return $"{timeSpan.TotalMilliseconds} ms";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
