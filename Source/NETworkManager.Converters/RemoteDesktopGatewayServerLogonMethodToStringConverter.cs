using NETworkManager.Models.RemoteDesktop;
using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization;

namespace NETworkManager.Converters;

/// <summary>
/// Convert <see cref="GatewayUserSelectedCredsSource"/> to translated <see cref="string"/> or wise versa.
/// </summary>
public sealed class RemoteDesktopGatewayServerLogonMethodToStringConverter : IValueConverter
{
    /// <summary>
    /// Convert <see cref="GatewayUserSelectedCredsSource"/> to translated <see cref="string"/>. 
    /// </summary>
    /// <param name="value">Object from type <see cref="GatewayUserSelectedCredsSource"/>.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Translated <see cref="GatewayUserSelectedCredsSource"/>.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not GatewayUserSelectedCredsSource gatewayUserSelectedCredsSource ? "-/-" : ResourceTranslator.Translate(ResourceIdentifier.RemoteDesktopGatewayServerLogonMethod, gatewayUserSelectedCredsSource);
    }

    /// <summary>
    /// !!! Method not implemented !!!
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
