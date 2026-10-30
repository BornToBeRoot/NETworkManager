using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert the <see cref="IPScannerDetectionMethod" /> of a host to a translated <see cref="string" />
///     (e.g. "Ping, TCP").
/// </summary>
public sealed class IPScannerDetectionMethodToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert the <see cref="IPScannerDetectionMethod" /> of a host to a translated <see cref="string" />.
    /// </summary>
    /// <param name="value">Probes the host answered as <see cref="IPScannerDetectionMethod" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter">
    ///     "Status" to get the status of the host ("Up (detected by Ping, TCP)" / "Down"), "List" to get the
    ///     translated probes as list (e.g. for labels).
    /// </param>
    /// <param name="culture"></param>
    /// <returns>Translated, comma-separated probes, "-/-" if the host didn't answer any probe.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var detectedBy = value is IPScannerDetectionMethod method ? method : IPScannerDetectionMethod.None;

        var names = GetNames(detectedBy);

        return parameter as string switch
        {
            "List" => names,
            "Status" => detectedBy == IPScannerDetectionMethod.None
                ? Strings.Down
                : string.Format(Strings.UpDetectedByX, string.Join(", ", names)),
            _ => detectedBy == IPScannerDetectionMethod.None ? "-/-" : string.Join(", ", names)
        };
    }

    /// <summary>
    ///     Gets the translated names of the probes a host answered.
    /// </summary>
    /// <param name="detectedBy">Probes the host answered.</param>
    /// <returns>Translated names, empty if the host didn't answer any probe.</returns>
    private static List<string> GetNames(IPScannerDetectionMethod detectedBy)
    {
        var names = new List<string>(4);

        if (detectedBy.HasFlag(IPScannerDetectionMethod.ICMP))
            names.Add(Strings.Ping);

        if (detectedBy.HasFlag(IPScannerDetectionMethod.TCP))
            names.Add(Strings.TCP);

        if (detectedBy.HasFlag(IPScannerDetectionMethod.NetBIOS))
            names.Add(Strings.NetBIOS);

        if (detectedBy.HasFlag(IPScannerDetectionMethod.ARP))
            names.Add(Strings.ARPNDP);

        return names;
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
