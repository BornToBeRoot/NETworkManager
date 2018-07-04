using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class PingTimeToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (IPStatus)values[0];


            if (status != IPStatus.Success && status != IPStatus.TtlExpired)
                return "-/-";

            long.TryParse(values[1].ToString(), out var time);

            return time == 0 ? "<1 ms" : $"{time} ms";

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
