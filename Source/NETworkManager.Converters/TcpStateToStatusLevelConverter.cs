using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;
using NETworkManager.Controls;

namespace NETworkManager.Converters;

/// <summary>
///     Convert <see cref="TcpState" /> to <see cref="StatusLevel" />: established = success, connecting (SYN sent /
///     received) = warning, all other states (listen, closing, time wait, ...) neutral.
/// </summary>
public sealed class TcpStateToStatusLevelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            TcpState.Established => StatusLevel.Success,
            TcpState.SynSent or TcpState.SynReceived => StatusLevel.Warning,
            _ => StatusLevel.None
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
