using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using NETworkManager.Views;

namespace NETworkManager.ViewModels;

/// <summary>
/// Formats the count of hosts up (reachable), down (unreachable) or paused (not running) within
/// a Ping Monitor group, selected via <c>ConverterParameter</c> ("Up", "Down", "Paused" - formats
/// "{count} {label}" using the label bound as the third value - or "PausedVisibility", which
/// instead returns a <see cref="Visibility"/> so the paused count can be hidden while zero).
/// </summary>
/// <remarks>
/// Bound as a <see cref="MultiBinding"/> with the <see cref="CollectionViewGroup"/> as the first
/// value and <see cref="PingMonitorHostViewModel.HostsChangeVersion"/> as the second. The second
/// value isn't used directly, it only forces re-evaluation whenever a host's
/// <see cref="PingMonitorViewModel.IsReachable"/>/<see cref="PingMonitorViewModel.IsRunning"/>
/// changes, since <see cref="CollectionViewGroup"/> itself only raises change notifications for
/// item add/remove, not for property changes on its items.
/// </remarks>
public sealed class PingMonitorGroupSummaryConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 0 || values[0] is not CollectionViewGroup group)
            return parameter as string == "PausedVisibility" ? Visibility.Collapsed : string.Empty;

        var hosts = group.Items.OfType<PingMonitorView>().Select(host => host.ViewModel).ToList();

        switch (parameter as string)
        {
            case "Up":
                return $"{hosts.Count(host => host.IsRunning && host.IsReachable)} {values[2]}";
            case "Down":
                return $"{hosts.Count(host => host.IsRunning && !host.IsReachable)} {values[2]}";
            case "Paused":
                return $"{hosts.Count(host => !host.IsRunning)} {values[2]}";
            case "PausedVisibility":
                return hosts.Any(host => !host.IsRunning) ? Visibility.Visible : Visibility.Collapsed;
            default:
                return string.Empty;
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
