using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class IPAddressArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return "-/-";

            if (!(value is IPAddress[] ipAddresses))
                return "-/-";

            var result = string.Empty;

            foreach (var ipAddr in ipAddresses)
            {
                if (!string.IsNullOrEmpty(result))
                    result += Environment.NewLine;

                result += ipAddr.ToString();
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
