using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;
/// <summary>
/// Multiplies a value by a factor given with the parameter.
/// </summary>
/// <remarks>
/// Useful for setting sizes relative to window size.
/// </remarks>
public class SizeFactorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            double theValue = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double factor = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            return theValue * factor;
        }
        catch (Exception e) when (e is FormatException or InvalidCastException or OverflowException)
        {
            return 0.0;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}