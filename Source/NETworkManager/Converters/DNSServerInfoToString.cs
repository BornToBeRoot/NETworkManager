using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Models.Network;
using NETworkManager.Resources.Localization;

namespace NETworkManager.Converters
{
    public sealed class DNSServerInfoToString : IValueConverter
    {
        /* Convert an MahApps.Metro.Accent (from wpf/xaml-binding) to a Brush to fill rectangle with color in a ComboBox */
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DNSServerInfo dnsServerInfo))
                return "-/-";

            return dnsServerInfo.UseWindowsDNSServer ? $"[{Strings.WindowsDNSSettings}]" : dnsServerInfo.Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
