using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using NETworkManager.Models.Firewall;

namespace NETworkManager.Converters;

/// <summary>
/// Convert a program reference to a string and vice versa.
/// </summary>
public class FirewallRuleProgramConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType == typeof(FirewallRuleProgram))
        {
            if (value is not string program)
                return null;
            if (string.IsNullOrWhiteSpace(program))
                return null;
            try
            {
                var exe = new FirewallRuleProgram(program);
                return exe;
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        if (targetType != typeof(string))
            return null;
        return value is not FirewallRuleProgram prog ? null : prog.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}