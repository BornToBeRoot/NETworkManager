using NETworkManager.Models.Network;
using System;
using System.Globalization;
using System.Windows.Data;
using Windows.Networking.Connectivity;

namespace NETworkManager.Converters
{
    public sealed class NetworkAuthenticationTypeToHumanReadableStringConverter : IValueConverter
    {
        /* Translate the name of the accent */
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is NetworkAuthenticationType type))
                return "-/-";
            
            return $"{WiFi.GetHumanReadableNetworkAuthenticationType(type)}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
