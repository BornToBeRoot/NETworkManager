using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert a result to the style of a status icon: a green check (<c>CheckRectangle</c>) or a red cross
///     (<c>ErrorRectangle</c>), like the action of a rule in the Firewall.
/// </summary>
public sealed class SuccessStyleConverter : IValueConverter
{
    /// <summary>
    ///     Convert a result to the style of a status icon.
    /// </summary>
    /// <param name="value">
    ///     <see cref="bool" /> (e.g. a port is open), or the <see cref="IPScannerDetectionMethod" /> of a host with
    ///     the probe as <paramref name="parameter" />.
    /// </param>
    /// <param name="targetType"></param>
    /// <param name="parameter">Probe as name of <see cref="IPScannerDetectionMethod" /> (e.g. "ICMP").</param>
    /// <param name="culture"></param>
    /// <returns>Style of the check for success / an answer of the probe, of the cross otherwise.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isSuccess = value switch
        {
            bool boolValue => boolValue,
            IPScannerDetectionMethod detectedBy =>
                Enum.TryParse<IPScannerDetectionMethod>(parameter as string, out var method) &&
                detectedBy.HasFlag(method),
            _ => false
        };

        return Application.Current.FindResource(isSuccess ? "CheckRectangle" : "ErrorRectangle");
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
