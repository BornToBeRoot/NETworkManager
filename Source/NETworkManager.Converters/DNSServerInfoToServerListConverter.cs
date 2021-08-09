using NETworkManager.Models.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class DNSServerInfoToServerListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DNSServerInfo info)
                return "-/-";

            List<string> list = new List<string>();

            if (info.UseDoHServer)
                info.DoHServers.ForEach(x => list.Add(x.URL));
            else
                info.Servers.ForEach(x => list.Add($"{x.Server}:{x.Port}"));

            return string.Join("; ", list);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
