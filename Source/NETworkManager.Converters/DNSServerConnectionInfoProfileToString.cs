using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Models.Network;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Converters;

public sealed class DNSServerConnectionInfoProfileToString : IValueConverter
{        
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DNSServerConnectionInfoProfile dnsServerInfo)
            return "-/-";

        return dnsServerInfo.UseWindowsDNSServer ? $"[{Strings.WindowsDNSSettings}]" : dnsServerInfo.Name;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
