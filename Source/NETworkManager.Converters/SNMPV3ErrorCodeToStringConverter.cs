using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert a <see cref="SNMPV3ErrorCode" /> to a translated <see cref="string" /> (e.g. "Authentication failed").
/// </summary>
public sealed class SNMPV3ErrorCodeToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert a <see cref="SNMPV3ErrorCode" /> to a translated <see cref="string" />.
    /// </summary>
    /// <param name="value">Error code as <see cref="SNMPV3ErrorCode" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Translated error, empty for <see cref="SNMPV3ErrorCode.None" />.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not SNMPV3ErrorCode errorCode || errorCode == SNMPV3ErrorCode.None)
            return string.Empty;

        return ResourceTranslator.Translate(ResourceIdentifier.SNMPV3ErrorCode, errorCode);
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
