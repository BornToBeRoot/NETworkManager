using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class WiFiNetworkRssiInDecibelMilliwattsToSignalStrengthStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double networkRssiInDecibelMilliwatts))
                return "-/-";

            return $"{networkRssiInDecibelMilliwatts} dBm";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
