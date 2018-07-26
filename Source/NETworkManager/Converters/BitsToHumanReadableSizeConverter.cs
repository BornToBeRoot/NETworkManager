using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class BitsToHumanReadableSizeConverter : IValueConverter
    {
        private readonly string[] _sizes = { "Bit/s", "KBit/s", "MBit/s", "GBit/s", "Tbit/s" };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "-/-";

            double.TryParse(value.ToString(), out var bits);

            var sizeCount = 0;

            while (bits >= 1000 && ++sizeCount < _sizes.Length)
                bits /= 1000;

            return $"{bits} {_sizes[sizeCount]}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
