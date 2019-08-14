using NETworkManager.Models.Network;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class WiFiChannelFrequencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int frequency))
                return "-/-";

            double ghz = WiFi.ConvertChannelFrequencyToGigahertz(frequency) > 5 ? 5 : 2.4;

            return $"{ghz} GHz / Channel {WiFi.GetChannelFromChannelFrequency(frequency)}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
