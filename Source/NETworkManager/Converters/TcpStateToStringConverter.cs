using NETworkManager.Models.Settings;
using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class TcpStateToStringConverter : IValueConverter
    {        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TcpState tcpState = (TcpState)value;

            string status = LocalizationManager.GetStringByKey("String_TcpState_" + tcpState.ToString());

            if (string.IsNullOrEmpty(status))
                return tcpState.ToString();

            return status;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
