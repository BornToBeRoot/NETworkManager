using NETworkManager.Models.Network;
using System;
using System.Globalization;
using System.Windows.Data;
using Windows.Devices.WiFi;

namespace NETworkManager.Converters;

public sealed class WiFiPhyKindToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not WiFiPhyKind phyKind ? "-/-" : $"{WiFi.GetHumandReadablePhyKind(phyKind)}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
