using NETworkManager.Models.Network;
using System;
using System.Globalization;
using System.Windows.Data;
using Windows.Devices.WiFi;

namespace NETworkManager.Converters
{
    public sealed class WiFiPhyKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is WiFiPhyKind phyKind))
                return "-/-";

            return $"{WiFi.GetHumandReadablePhyKind(phyKind)} ({phyKind})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
