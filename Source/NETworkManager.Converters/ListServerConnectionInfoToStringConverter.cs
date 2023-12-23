using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

public sealed class ListServerConnectionInfoToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;

        StringBuilder stringBuilder = new();

        foreach (var server in (List<ServerConnectionInfo>)value)
        {
            if (stringBuilder.Length > 0)
                stringBuilder.Append("; ");

            stringBuilder.Append(server);
        }

        return stringBuilder.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}