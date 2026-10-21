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
    /// <summary>
    ///     Convert a hostname to <see cref="bool" />.
    /// </summary>
    /// <param name="value">Hostname as <see cref="string" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>True if the hostname has a domain, otherwise false.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string hostname && hostname.Trim('.').Contains('.');
    }

    /// <summary>
    ///     !!! Method not implemented !!!
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
