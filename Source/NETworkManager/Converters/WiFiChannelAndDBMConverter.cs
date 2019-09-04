using NETworkManager.Models.Network;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class WiFiChannelAndDBMConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is WiFiNetworkInfo info))
                return "-/-";

            double ghz = WiFi.ConvertChannelFrequencyToGigahertz(info.ChannelCenterFrequencyInKilohertz) > 5 ? 5 : 2.4;

            return $"{ghz} GHz / Channel {WiFi.GetChannelFromChannelFrequency(info.ChannelCenterFrequencyInKilohertz)} ({info.NetworkRssiInDecibelMilliwatts} dBm)";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
