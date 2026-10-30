using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert the <see cref="IPScannerOSGuess" /> and the hops of a host to a translated <see cref="string" />.
/// </summary>
public sealed class IPScannerOSGuessToStringConverter : IMultiValueConverter
{
    /// <summary>
    ///     Convert the <see cref="IPScannerOSGuess" /> and the hops of a host to a translated <see cref="string" />.
    /// </summary>
    /// <param name="values"><see cref="IPScannerOSGuess" /> and the hops (<see cref="int" />).</param>
    /// <param name="targetType"></param>
    /// <param name="parameter">"ToolTip" to get the tooltip ("Based on the TTL of the ping reply (2 hops)").</param>
    /// <param name="culture"></param>
    /// <returns>Translated operating system ("-/-" without guess), or the tooltip (null without guess).</returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var osGuess = values.Length > 0 && values[0] is IPScannerOSGuess value ? value : IPScannerOSGuess.None;

        if (parameter as string != "ToolTip")
            return osGuess == IPScannerOSGuess.None ? "-/-" : ToLocalizedString(osGuess);

        if (osGuess == IPScannerOSGuess.None)
            return null;

        return string.Format(Strings.OSGuessBasedOnTheTTLXHops, values.Length > 1 && values[1] is int hops ? hops : 0);
    }

    /// <summary>
    ///     Gets the translated name of a guessed operating system.
    /// </summary>
    /// <param name="osGuess">Guessed operating system.</param>
    /// <returns>Translated name, empty without guess.</returns>
    public static string ToLocalizedString(IPScannerOSGuess osGuess)
    {
        return osGuess switch
        {
            IPScannerOSGuess.LinuxUnix => Strings.LinuxUnix,
            IPScannerOSGuess.Windows => Strings.Windows,
            IPScannerOSGuess.NetworkDevice => Strings.NetworkDevice,
            _ => string.Empty
        };
    }

    /// <summary>
    ///     !!! Method not implemented !!!
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetTypes"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
