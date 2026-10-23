using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Converters;

/// <summary>
///     Convert a <see cref="bool" /> that indicates if a host is reachable to a translated <see cref="string" />
///     ("Up" / "Down").
/// </summary>
public sealed class HostReachableToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert a <see cref="bool" /> that indicates if a host is reachable to a translated <see cref="string" />.
    /// </summary>
    /// <param name="value">Host reachable as <see cref="bool" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Translated "Up" or "Down".</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? Strings.Up : Strings.Down;
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
