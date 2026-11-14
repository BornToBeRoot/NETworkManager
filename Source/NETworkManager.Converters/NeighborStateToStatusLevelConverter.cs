using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Controls;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert <see cref="NeighborState" /> to <see cref="StatusLevel" />: reachable = success, stale / delay / probe
///     (not confirmed recently) = warning, incomplete / unreachable = error, permanent (static entry) neutral.
/// </summary>
public sealed class NeighborStateToStatusLevelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            NeighborState.Reachable => StatusLevel.Success,
            NeighborState.Stale or NeighborState.Delay or NeighborState.Probe => StatusLevel.Warning,
            NeighborState.Incomplete or NeighborState.Unreachable => StatusLevel.Error,
            _ => StatusLevel.None
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
