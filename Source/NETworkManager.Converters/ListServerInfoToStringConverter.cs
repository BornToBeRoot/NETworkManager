using NETworkManager.Models.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Text;

namespace NETworkManager.Converters
{
    public sealed class ListServerInfoToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            StringBuilder stringBuilder = new();
            
            foreach (var server in (List<ServerInfo>)value)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append("; ");
                
                stringBuilder.Append(server.ToString());
            }

            return stringBuilder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
