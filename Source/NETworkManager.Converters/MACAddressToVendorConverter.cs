using NETworkManager.Models.Lookup;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class MACAddressToVendorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string macAddress))
                return "-/-";

            return OUILookup.Lookup(macAddress).FirstOrDefault()?.Vendor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
