using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
/// Convert a <see cref="bool"/> that indicates if a port is open to a translated <see cref="string"/> or wise versa.
/// </summary>
public sealed class PortOpenToStringConverter : IValueConverter
{
    /// <summary>
    /// Convert a <see cref="bool"/> that indicates if a port is open to a translated <see cref="string"/>. 
    /// </summary>
    /// <param name="value">Port up as <see cref="bool"/>.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Translated <see cref="PortState"/>.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not bool portState ? "-/-" : ResourceTranslator.Translate(ResourceIdentifier.PortState, portState ? PortState.Open : PortState.Closed);
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
