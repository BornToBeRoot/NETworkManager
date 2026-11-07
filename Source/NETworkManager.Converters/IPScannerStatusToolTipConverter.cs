using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert a result of the IP Scanner to a tooltip: the open ports (Open ports column), or whether the host answered
///     a probe (status icons in the headers of the row details).
/// </summary>
public sealed class IPScannerStatusToolTipConverter : IValueConverter
{
    /// <summary>
    ///     Maximum number of open ports listed in the tooltip.
    /// </summary>
    private const int MaxPorts = 10;

    /// <summary>
    ///     Convert a result of the IP Scanner to the tooltip of its status icon.
    /// </summary>
    /// <param name="value">
    ///     Ports (<see cref="List{PortInfo}" />), or the <see cref="IPScannerDetectionMethod" /> of a host with the probe
    ///     as <paramref name="parameter" />.
    /// </param>
    /// <param name="targetType"></param>
    /// <param name="parameter">Probe as name of <see cref="IPScannerDetectionMethod" /> (e.g. "ICMP").</param>
    /// <param name="culture"></param>
    /// <returns>Tooltip text.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (value)
        {
            // Open: 22 (ssh), 80 (http) / No open ports found! (closed or timed out - the ports that are not open may
            // be hidden, see IPScanner.GetShownPorts)
            case List<PortInfo> ports:
                var openPorts = ports.Where(x => x.State == PortState.Open).ToList();

                if (openPorts.Count == 0)
                    return Strings.NoOpenPortsFound;

                var list = string.Join(", ", openPorts.Take(MaxPorts).Select(x =>
                    string.IsNullOrEmpty(x.LookupInfo?.Service) ? $"{x.Port}" : $"{x.Port} ({x.LookupInfo.Service})"));

                if (openPorts.Count > MaxPorts)
                    list += " " + string.Format(Strings.PlusXMore, openPorts.Count - MaxPorts);

                return $"{ResourceTranslator.Translate(ResourceIdentifier.PortState, PortState.Open)}: {list}";

            // Answered / No answer
            case IPScannerDetectionMethod detectedBy:
                return Enum.TryParse<IPScannerDetectionMethod>(parameter as string, out var method) &&
                       detectedBy.HasFlag(method)
                    ? Strings.Answered
                    : Strings.NoAnswer;

            default:
                return null;
        }
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
