using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Data;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert a result of the IP Scanner to the tooltip of its status icon: ping status with time and TTL, open ports,
///     NetBIOS name and workgroup / domain - or whether the host answered a probe.
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
    ///     <see cref="PingInfo" />, ports (<see cref="List{PortInfo}" />), <see cref="NetBIOSInfo" />, or the
    ///     <see cref="IPScannerDetectionMethod" /> of a host with the probe as <paramref name="parameter" />.
    /// </param>
    /// <param name="targetType"></param>
    /// <param name="parameter">Probe as name of <see cref="IPScannerDetectionMethod" /> (e.g. "ICMP").</param>
    /// <param name="culture"></param>
    /// <returns>Tooltip text.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (value)
        {
            // Success (2 ms, TTL 64) / Timed out
            case PingInfo pingInfo:
                var status = ResourceTranslator.Translate(ResourceIdentifier.IPStatus, pingInfo.Status);

                if (pingInfo.Status != IPStatus.Success)
                    return status;

                var time = Models.Network.Ping.TimeToString(pingInfo.Status, pingInfo.Time);

                // No TTL for IPv6
                return pingInfo.TTL > 0 ? $"{status} ({time}, TTL {pingInfo.TTL})" : $"{status} ({time})";

            // Open: 22 (ssh), 80 (http) / Closed
            case List<PortInfo> ports:
                var openPorts = ports.Where(x => x.State == PortState.Open).ToList();

                if (openPorts.Count == 0)
                    return ResourceTranslator.Translate(ResourceIdentifier.PortState, PortState.Closed);

                var list = string.Join(", ", openPorts.Take(MaxPorts).Select(x =>
                    string.IsNullOrEmpty(x.LookupInfo?.Service) ? $"{x.Port}" : $"{x.Port} ({x.LookupInfo.Service})"));

                if (openPorts.Count > MaxPorts)
                    list += " " + string.Format(Strings.PlusXMore, openPorts.Count - MaxPorts);

                return $"{ResourceTranslator.Translate(ResourceIdentifier.PortState, PortState.Open)}: {list}";

            // Yes (PC01, WORKGROUP) / No
            case NetBIOSInfo netBIOSInfo:
                if (!netBIOSInfo.IsReachable)
                    return Strings.No;

                var names = new[] { netBIOSInfo.ComputerName, netBIOSInfo.GroupName }
                    .Where(x => !string.IsNullOrEmpty(x)).ToList();

                return names.Count == 0 ? Strings.Yes : $"{Strings.Yes} ({string.Join(", ", names)})";

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
