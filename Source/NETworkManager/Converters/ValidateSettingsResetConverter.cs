using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using NETworkManager.Utilities;

namespace NETworkManager.Converters
{
    public sealed class ValidateSettingsResetConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return (!(bool)values[0] && ArrayHelper.SubArray(values, 1, 7).Any(x => (bool)x) == true);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
