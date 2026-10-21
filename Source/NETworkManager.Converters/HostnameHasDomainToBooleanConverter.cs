using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;

/// <summary>
///     Convert a hostname to <see cref="bool" />: true if it has a domain (e.g. "server01.borntoberoot.net"), false for
///     a single label like a NetBIOS name ("SERVER01").
/// </summary>
public sealed class HostnameHasDomainToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string hostname && hostname.Trim('.').Contains('.');
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
