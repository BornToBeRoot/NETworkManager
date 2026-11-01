using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Text and tooltip of the OS column of the IP Scanner: the Windows version if it was read (more precise), otherwise
///     the guess from the TTL of the ping reply (see <see cref="IPScannerOSGuessToStringConverter" />).
/// </summary>
public sealed class IPScannerOSToStringConverter : IMultiValueConverter
{
    /// <summary>
    ///     Gets the text or tooltip of the OS column.
    /// </summary>
    /// <param name="values">
    ///     [0] <see cref="IPScannerOSGuess" />, [1] hops (<see cref="int" />), [2] the ping is running, [3] the guess was
    ///     enabled for the scan, [4] <see cref="WindowsHostInfo" /> (null if not read), [5] the Windows host information
    ///     is still read.
    /// </param>
    /// <param name="targetType"></param>
    /// <param name="parameter">"ToolTip" to get the tooltip (version of Windows, or the hops of the guess).</param>
    /// <param name="culture"></param>
    /// <returns>Text ("..." while running, "-/-" without value) or tooltip (null without value).</returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not { Length: 6 })
            return null;

        var osGuess = values[3] is true && values[0] is IPScannerOSGuess value ? value : IPScannerOSGuess.None;
        var hops = values[1] is int hopsValue ? hopsValue : 0;
        var isPingPending = values[3] is true && values[2] is true;
        var windows = values[4] as WindowsHostInfo;
        var isWindowsPending = values[5] is true;

        var isToolTip = parameter as string == "ToolTip";

        if (!string.IsNullOrEmpty(windows?.OS))
        {
            if (!isToolTip)
                return windows.OS;

            // E.g. "Pro, 10.0.26200.6584"
            return string.IsNullOrEmpty(windows.Edition)
                ? windows.Version?.ToString()
                : $"{windows.Edition}, {windows.Version}";
        }

        if (osGuess != IPScannerOSGuess.None)
            return isToolTip
                ? string.Format(Strings.OSGuessBasedOnTheTTLXHops, hops)
                : IPScannerOSGuessToStringConverter.ToLocalizedString(osGuess);

        if (isToolTip)
            return null;

        return isPingPending || isWindowsPending ? "..." : "-/-";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
