using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Models.Settings;
using static NETworkManager.Models.Network.PortInfo;

namespace NETworkManager.Converters
{
    public sealed class PortStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is PortStatus portStatus))
                return "-/-";

            var status = LocalizationManager.TranslatePortStatus(portStatus);

            return string.IsNullOrEmpty(status) ? portStatus.ToString() : status;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
