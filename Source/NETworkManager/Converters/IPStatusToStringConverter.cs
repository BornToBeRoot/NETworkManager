using NETworkManager.Models.Settings;
using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class IpStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is IPStatus ipStatus))
                return "-/-";

            var status = LocalizationManager.GetStringByKey("String_IPStatus_" + ipStatus);

            return string.IsNullOrEmpty(status) ? ipStatus.ToString() : status;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
