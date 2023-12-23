using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert <see cref="DiscoveryProtocol" /> to translated <see cref="string" /> or wise versa.
/// </summary>
public sealed class DiscoveryProtocolToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert <see cref="DiscoveryProtocol" /> to translated <see cref="string" />.
    /// </summary>
    /// <param name="value">Object from type <see cref="DiscoveryProtocol" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Translated <see cref="DiscoveryProtocol" />.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not DiscoveryProtocol discoveryProtocol
            ? "-/-"
            : ResourceTranslator.Translate(ResourceIdentifier.DiscoveryProtocol,
                discoveryProtocol);
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