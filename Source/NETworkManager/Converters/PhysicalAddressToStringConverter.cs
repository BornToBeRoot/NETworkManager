using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Helpers;

namespace NETworkManager.Converters
{
    public sealed class PhysicalAddressToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string macAddress = value as string;

            if(string.IsNullOrEmpty(macAddress))
                return string.Empty;

            return MACAddressHelper.GetDefaultFormat(macAddress);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
