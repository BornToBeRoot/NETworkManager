using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Data;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

public sealed class IPAddressArrayToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            return "-/-";

        if (value is not IPAddress[] ipAddresses)
            return "-/-";

        return IPv4Address.ConvertIPAddressListToString(ipAddresses);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}