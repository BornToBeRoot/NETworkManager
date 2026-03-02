using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace NETworkManager.Converters;

public sealed class BooleansOrConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values?.OfType<bool>().Any(value => value) ?? false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}