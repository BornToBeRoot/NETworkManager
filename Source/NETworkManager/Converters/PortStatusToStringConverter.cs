using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static NETworkManager.Models.Network.PortInfo;

namespace NETworkManager.Converters
{
    public sealed class PortStatusToStringConverter : IValueConverter
    {        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PortStatus portStatus = (PortStatus)value;

            string status = Application.Current.Resources["String_PortStatus_" + portStatus.ToString()] as string;

            if (string.IsNullOrEmpty(status))
                return portStatus.ToString();

            return status;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
