using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Utilities;
using System.Net.NetworkInformation;

namespace NETworkManager.Converters
{
    public sealed class PhysicalAddressToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? string.Empty : MACAddressHelper.GetDefaultFormat((PhysicalAddress)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
