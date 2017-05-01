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
            IPStatus status = (IPStatus)values[0];

            if (status == IPStatus.Success || status == IPStatus.TtlExpired)
            {
                long time;

                long.TryParse(values[1].ToString(), out time);

                if (time == 0)
                    return string.Format("<1 ms");
                else
                    return string.Format("{0} ms", time);
            }

            return string.Format("-/-");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
