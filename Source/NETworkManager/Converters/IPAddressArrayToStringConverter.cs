using System;
using System.Globalization;
using System.Net;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class IPAddressArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            
            if (value == null)
                return result;

            foreach (IPAddress ipAddr in value as IPAddress[])
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
