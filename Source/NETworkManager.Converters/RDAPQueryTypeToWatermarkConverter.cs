using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.RDAP;

namespace NETworkManager.Converters;

/// <summary>
///     Convert <see cref="RDAPQueryType" /> to an example input (watermark).
/// </summary>
public sealed class RDAPQueryTypeToWatermarkConverter : IValueConverter
{
    /// <summary>
    ///     Convert <see cref="RDAPQueryType" /> to an example input (watermark).
    /// </summary>
    /// <param name="value">Object from type <see cref="RDAPQueryType" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Example input for the <see cref="RDAPQueryType" />.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            RDAPQueryType.TLD => StaticStrings.ExampleTLD,
            RDAPQueryType.IPAddress => StaticStrings.ExampleIPAddressOrCIDR,
            RDAPQueryType.ASN => StaticStrings.ExampleASN,
            RDAPQueryType.Entity => StaticStrings.ExampleRDAPEntityHandle,
            _ => StaticStrings.ExampleDomain
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
