using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert the <see cref="IPScannerDetectionMethod" /> of a host to "Up" / "Down" for one probe (e.g. the label in
///     the header of the ping details).
/// </summary>
public sealed class IPScannerDetectionMethodToUpDownConverter : IValueConverter
{
    /// <summary>
    ///     Convert the <see cref="IPScannerDetectionMethod" /> of a host to "Up" if it answered the probe, else "Down".
    /// </summary>
    /// <param name="value">Probes the host answered as <see cref="IPScannerDetectionMethod" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter">Probe as name of <see cref="IPScannerDetectionMethod" /> (e.g. "ICMP").</param>
    /// <param name="culture"></param>
    /// <returns>Translated "Up" or "Down".</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IPScannerDetectionMethod detectedBy ||
            !Enum.TryParse<IPScannerDetectionMethod>(parameter as string, out var method))
            return Strings.Down;

        return detectedBy.HasFlag(method) ? Strings.Up : Strings.Down;
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
