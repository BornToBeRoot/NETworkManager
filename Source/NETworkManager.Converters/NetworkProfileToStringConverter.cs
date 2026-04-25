using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert <see cref="NetworkProfile" /> to a localized <see cref="string" /> or vice versa.
/// </summary>
public sealed class NetworkProfileToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert <see cref="NetworkProfile" /> to a localized <see cref="string" />.
    /// </summary>
    /// <param name="value">Object from type <see cref="NetworkProfile" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Localized <see cref="string" /> representing the network profile.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not NetworkProfile profile
            ? "-/-"
            : profile switch
            {
                NetworkProfile.Domain => Strings.Domain,
                NetworkProfile.Private => Strings.Private,
                NetworkProfile.Public => Strings.Public,
                _ => "-/-"
            };
    }

    /// <summary>
    ///     !!! Method not implemented !!!
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
