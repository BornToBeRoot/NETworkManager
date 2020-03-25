using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Utilities;

namespace NETworkManager.Converters
{
    public sealed class Bytes1000ToSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? $"{FileSizeConverter.GetBytesReadable((long)value)}" : "-/-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
