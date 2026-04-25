using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization;
using NETworkManager.Models.Firewall;

namespace NETworkManager.Converters;

/// <summary>
///     Convert <see cref="FirewallRuleAction" /> to translated <see cref="string" /> or vice versa.
/// </summary>
public sealed class FirewallRuleActionToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert <see cref="FirewallRuleAction" /> to translated <see cref="string" />.
    /// </summary>
    /// <param name="value">Object from type <see cref="FirewallRuleAction" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Translated <see cref="FirewallRuleAction" />.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not FirewallRuleAction action
            ? "-/-"
            : ResourceTranslator.Translate(ResourceIdentifier.FirewallRuleAction, action);
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