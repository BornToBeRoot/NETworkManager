using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;

public sealed class WiFiHiddenSsidToStringConverter : IValueConverter
{
    /* Translate the name of the accent */
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string ssid)
            return "-/-";

        return string.IsNullOrEmpty(ssid) ? Localization.Resources.Strings.HiddenNetwork : ssid;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
