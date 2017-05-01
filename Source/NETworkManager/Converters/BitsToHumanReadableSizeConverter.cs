using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class BitsToHumanReadableSizeConverter : IValueConverter
    {
        string[] sizes = { "Bit/s", "KBit/s", "MBit/s", "GBit/s", "Tbit/s" };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double bits = 0;
            double.TryParse(value.ToString(), out bits);

            int sizeCount = 0;

            while (bits >= 1000 && ++sizeCount < sizes.Length)
            {
                bits /= 1000;
            }

            return string.Format("{0} {1}", bits, sizes[sizeCount]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
