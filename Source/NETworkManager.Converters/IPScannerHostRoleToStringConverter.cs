using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert the <see cref="IPScannerHostRole" /> of a host to a translated <see cref="string" />
///     (e.g. "Default gateway, DNS server").
/// </summary>
public sealed class IPScannerHostRoleToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert the <see cref="IPScannerHostRole" /> of a host to a translated <see cref="string" />.
    /// </summary>
    /// <param name="value">Roles of the host as <see cref="IPScannerHostRole" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Translated, comma-separated roles, null if the host has none (no tooltip).</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IPScannerHostRole role || role == IPScannerHostRole.None)
            return null;

        return ToLocalizedString(role);
    }

    /// <summary>
    ///     Gets the translated, comma-separated roles of a host.
    /// </summary>
    /// <param name="role">Roles of the host.</param>
    /// <returns>Translated roles, empty if the host has none.</returns>
    public static string ToLocalizedString(IPScannerHostRole role)
    {
        var names = new List<string>(4);

        if (role.HasFlag(IPScannerHostRole.LocalComputer))
            names.Add(Strings.ThisComputer);

        if (role.HasFlag(IPScannerHostRole.DefaultGateway))
            names.Add(Strings.DefaultGateway);

        if (role.HasFlag(IPScannerHostRole.DNSServer))
            names.Add(Strings.DNSServer);

        if (role.HasFlag(IPScannerHostRole.DHCPServer))
            names.Add(Strings.DHCPServer);

        return string.Join(", ", names);
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
