using NETworkManager.Models.Network;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class WiFiGHzToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int channelCenterFrequencyInKilohertz))
                return "-/-";

            return $"{WiFi.ConvertChannelFrequencyToGigahertz(channelCenterFrequencyInKilohertz)} GHz";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
