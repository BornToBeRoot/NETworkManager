using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Helpers;
using System.Net.NetworkInformation;

namespace NETworkManager.Converters
{
    public sealed class PhysicalAddressToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            return MACAddressHelper.GetDefaultFormat((PhysicalAddress)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
