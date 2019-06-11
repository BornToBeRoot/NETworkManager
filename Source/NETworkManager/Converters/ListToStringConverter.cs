using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? string.Empty : string.Join("; ", (List<string>)value);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
