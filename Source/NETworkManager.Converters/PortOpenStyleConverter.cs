using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

public sealed class PortOpenStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not PortState portState)
            return null;

        return portState == PortState.Open
            ? Application.Current.FindResource("PortOpenRectangle")
            : Application.Current.FindResource("PortClosedRectangle");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}