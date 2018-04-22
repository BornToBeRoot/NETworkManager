using NETworkManager.Models.Settings;
using System;
using System.Globalization;
using System.Windows.Data;
using static NETworkManager.Models.Network.PortInfo;

namespace NETworkManager.Converters
{
    public sealed class PortStatusToStringConverter : IValueConverter
    {        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PortStatus portStatus = (PortStatus)value;

            string status = LocalizationManager.GetStringByKey("String_PortStatus_" + portStatus.ToString());

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
