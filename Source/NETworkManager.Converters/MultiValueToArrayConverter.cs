using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;

/// <summary>
/// Passes the values of a <see cref="MultiBinding"/> as an array, e.g. a command parameter with several values (an
/// application and the host of a group header in the Port Scanner).
/// </summary>
public sealed class MultiValueToArrayConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // Copy - the array of the MultiBinding is reused
        return values.Clone();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
