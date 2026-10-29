using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Profiles;

namespace NETworkManager.Converters;

/// <summary>
///     Convert a <see cref="ProfileInfo" /> to the ports the Port Scanner scans for it (own ports, otherwise the ports
///     of its group, port profiles with their current name).
/// </summary>
public sealed class ProfileToPortScannerPortsConverter : IValueConverter
{
    /// <summary>
    ///     Convert a <see cref="ProfileInfo" /> to the ports to scan (see
    ///     <see cref="Profiles.Application.PortScanner.GetPorts" />).
    /// </summary>
    /// <param name="value">Object from type <see cref="ProfileInfo" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Ports to scan, empty if neither the profile nor its group has ports.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not ProfileInfo profile
            ? string.Empty
            : Profiles.Application.PortScanner.GetPorts(profile, out _);
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
