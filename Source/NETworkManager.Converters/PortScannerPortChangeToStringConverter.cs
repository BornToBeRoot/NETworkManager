using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert the <see cref="PortScannerPortChange" /> of a port (since the previous scan) to a translated
///     <see cref="string" /> (e.g. "Now open").
/// </summary>
public sealed class PortScannerPortChangeToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert the <see cref="PortScannerPortChange" /> of a port to a translated <see cref="string" />.
    /// </summary>
    /// <param name="value">Change of the port as <see cref="PortScannerPortChange" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Translated change, empty if there is none.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is PortScannerPortChange change ? ToLocalizedString(change) : string.Empty;
    }

    /// <summary>
    ///     Gets the translated change of a port.
    /// </summary>
    /// <param name="change">Change of the port.</param>
    /// <returns>Translated change, empty for <see cref="PortScannerPortChange.None" />.</returns>
    public static string ToLocalizedString(PortScannerPortChange change)
    {
        return change switch
        {
            PortScannerPortChange.Opened => Strings.PortChange_Opened,
            PortScannerPortChange.Closed => Strings.PortChange_Closed,
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
