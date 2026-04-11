using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Converters;

/// <summary>
///     Convert a <see cref="bool" /> array (Domain, Private, Public) representing firewall network
///     profiles to a localized <see cref="string" />, or vice versa.
/// </summary>
public sealed class FirewallNetworkProfilesToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert a <see cref="bool" /> array (Domain, Private, Public) to a localized <see cref="string" />.
    ///     Returns <c>null</c> when all three profiles are active so that a <c>TargetNullValue</c> binding
    ///     can supply the translated "Any" label.
    /// </summary>
    /// <param name="value">A <see cref="bool" /> array with exactly three elements.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Localized, comma-separated profile list (e.g. "Domain, Private, Public").</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool[] { Length: 3 } profiles)
            return "-/-";

        var names = new List<string>(3);
        if (profiles[0]) names.Add(Strings.Domain);
        if (profiles[1]) names.Add(Strings.Private);
        if (profiles[2]) names.Add(Strings.Public);

        return names.Count == 0 ? "-" : string.Join(", ", names);
    }

    /// <summary>
    ///     !!! Method not implemented !!!
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
