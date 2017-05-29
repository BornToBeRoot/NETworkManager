using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;
using NETworkManager.Helpers;

namespace NETworkManager.Converters
{
    public sealed class PhysicalAddressToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            return MACAddressHelper.GetDefaultFormat((value as PhysicalAddress).ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
