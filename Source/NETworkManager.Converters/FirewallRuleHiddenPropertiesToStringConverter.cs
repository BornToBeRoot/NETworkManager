using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Firewall;

namespace NETworkManager.Converters;

/// <summary>
///     Convert <see cref="FirewallRuleHiddenProperties" /> to a translated, comma-separated <see cref="string" />
///     (e.g. "ICMP types, Service").
/// </summary>
public sealed class FirewallRuleHiddenPropertiesToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert <see cref="FirewallRuleHiddenProperties" /> to a translated, comma-separated <see cref="string" />.
    /// </summary>
    /// <param name="value">Object from type <see cref="FirewallRuleHiddenProperties" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Translated <see cref="FirewallRuleHiddenProperties" />.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not FirewallRuleHiddenProperties properties
            ? "-/-"
            : string.Join(", ", GetNames(properties));
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

    /// <summary>
    ///     Gets the translated names of the hidden properties (e.g. "ICMP types", "Service").
    ///     Also used for the warning shown before a rule is copied.
    /// </summary>
    /// <param name="properties">The hidden properties.</param>
    /// <returns>The translated names.</returns>
    public static List<string> GetNames(FirewallRuleHiddenProperties properties)
    {
        var names = new List<string>();

        if (properties.HasFlag(FirewallRuleHiddenProperties.IcmpTypes))
            names.Add(Strings.IcmpTypes);

        if (properties.HasFlag(FirewallRuleHiddenProperties.Service))
            names.Add(Strings.Service);

        if (properties.HasFlag(FirewallRuleHiddenProperties.Package))
            names.Add(Strings.AppPackage);

        if (properties.HasFlag(FirewallRuleHiddenProperties.InterfaceAlias))
            names.Add(Strings.NetworkAdapters);

        if (properties.HasFlag(FirewallRuleHiddenProperties.Security))
            names.Add(Strings.UsersAndAuthentication);

        return names;
    }
}
