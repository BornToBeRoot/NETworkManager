using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using NETworkManager.Models.Firewall;
using NETworkManager.Settings;

namespace NETworkManager.Converters;

/// <summary>
/// Converts a port range or port to an instance of FirewallPortSpecification. 
/// </summary>
public class PortRangeToPortSpecificationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        char portDelimiter = SettingsManager.Current.Firewall_UseWindowsPortSyntax ? ',' : ';';
        if (targetType == typeof(List<FirewallPortSpecification>))
        {
            if (value is not string input)
                return null;
            const char rangeDelimiter = '-';
            List<FirewallPortSpecification> resultList = [];
            var portList = input.Split(portDelimiter);
            foreach (var port in portList)
            {
                if (port.Contains(rangeDelimiter))
                {
                    var portRange = port.Split(rangeDelimiter);
                    if (!int.TryParse(portRange[0], out var startPort))
                        return null;
                    if (!int.TryParse(portRange[1], out var endPort))
                        return null;
                    resultList.Add(new FirewallPortSpecification(startPort, endPort));
                }
                else
                {
                    if (!int.TryParse(port, out var portNumber))
                        return null;
                    resultList.Add(new FirewallPortSpecification(portNumber));
                }
            }
            return resultList;
        }

        if (targetType != typeof(string))
            return null;
        if (value is not List<FirewallPortSpecification> portSpecs)
            return string.Empty;
        string result = portSpecs
            .Aggregate("", (current, portSpecification) => current + $"{portSpecification}{portDelimiter} ");
        return result.Length > 0 ? result[..^2] : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}