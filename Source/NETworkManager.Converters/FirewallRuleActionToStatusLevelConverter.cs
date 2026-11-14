using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Controls;
using NETworkManager.Models.Firewall;

namespace NETworkManager.Converters;

/// <summary>
///     Convert <see cref="FirewallRuleAction" /> to <see cref="StatusLevel" />: allow = success, block = error.
/// </summary>
public sealed class FirewallRuleActionToStatusLevelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            FirewallRuleAction.Allow => StatusLevel.Success,
            FirewallRuleAction.Block => StatusLevel.Error,
            _ => StatusLevel.None
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
