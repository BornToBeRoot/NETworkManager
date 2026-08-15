using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
/// Formats the count of hosts up (reachable), down (unreachable) or paused (not running) within
/// a Ping Monitor group, selected via <c>ConverterParameter</c> ("Up", "Down", "Paused" - formats
/// "{count} {label}" using the label bound as the third value - or "PausedVisibility", which
/// instead returns a <see cref="Visibility"/> so the paused count can be hidden while zero).
/// </summary>
/// <remarks>
/// Bound as a <see cref="MultiBinding"/> with the <see cref="CollectionViewGroup"/> as the first
/// value and a per-group change-notification trigger as the second. The second value isn't used
/// directly, it only forces re-evaluation whenever a host's <see cref="IPingMonitorHostStatus"/>
/// changes, since <see cref="CollectionViewGroup"/> itself only raises change notifications for
/// item add/remove, not for property changes on its items.
/// </remarks>
public sealed class PingMonitorGroupSummaryConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 0 || values[0] is not CollectionViewGroup group)
            return parameter as string == "PausedVisibility" ? Visibility.Collapsed : string.Empty;

        var up = 0;
        var down = 0;
        var paused = 0;

        foreach (var host in group.Items.OfType<IPingMonitorHostStatus>())
        {
            if (!host.IsRunning)
                paused++;
            else if (host.IsReachable)
                up++;
            else
                down++;
        }

        return parameter as string switch
        {
            "Up" => $"{up} {values[2]}",
            "Down" => $"{down} {values[2]}",
            "Paused" => $"{paused} {values[2]}",
            "PausedVisibility" => paused > 0 ? Visibility.Visible : Visibility.Collapsed,
            _ => string.Empty
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
