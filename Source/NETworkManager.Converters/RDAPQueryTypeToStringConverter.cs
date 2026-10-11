using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization;
using NETworkManager.Models.RDAP;

namespace NETworkManager.Converters;

/// <summary>
///     Convert <see cref="RDAPQueryType" /> to translated <see cref="string" />.
/// </summary>
public sealed class RDAPQueryTypeToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert <see cref="RDAPQueryType" /> to translated <see cref="string" />.
    /// </summary>
    /// <param name="value">Object from type <see cref="RDAPQueryType" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Translated <see cref="RDAPQueryType" />.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not RDAPQueryType queryType
            ? "-/-"
            : ResourceTranslator.Translate(ResourceIdentifier.RDAPQueryType, queryType);
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
