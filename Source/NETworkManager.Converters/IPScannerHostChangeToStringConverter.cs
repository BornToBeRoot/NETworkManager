using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert the <see cref="IPScannerHostChange" /> of a host (since the previous scan) to a translated
///     <see cref="string" /> (e.g. "New").
/// </summary>
public sealed class IPScannerHostChangeToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert the <see cref="IPScannerHostChange" /> of a host to a translated <see cref="string" />.
    /// </summary>
    /// <param name="value">Change of the host as <see cref="IPScannerHostChange" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Translated change, empty if there is none.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is IPScannerHostChange change ? ToLocalizedString(change) : string.Empty;
    }

    /// <summary>
    ///     Gets the translated change of a host.
    /// </summary>
    /// <param name="change">Change of the host.</param>
    /// <returns>Translated change, empty for <see cref="IPScannerHostChange.None" />.</returns>
    public static string ToLocalizedString(IPScannerHostChange change)
    {
        return change switch
        {
            IPScannerHostChange.New => Strings.HostChange_New,
            IPScannerHostChange.Gone => Strings.HostChange_Gone,
            IPScannerHostChange.MACChanged => Strings.HostChange_MACChanged,
            _ => string.Empty
        };
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
