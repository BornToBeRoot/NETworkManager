using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Data;

namespace NETworkManager.Views.Converters
{
    public sealed class IPStatusToStringConverter : IValueConverter
    {        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IPStatus ipStatus = (IPStatus)value;

            string status = Application.Current.Resources["String_IPStatus_" + ipStatus.ToString()] as string;

            if (string.IsNullOrEmpty(status))
                return ipStatus.ToString();

            return status;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
