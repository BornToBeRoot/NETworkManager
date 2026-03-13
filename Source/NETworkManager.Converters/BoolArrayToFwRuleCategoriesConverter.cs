using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using NETworkManager.Models.Network;
using NETworkManager.Localization.Resources;


namespace NETworkManager.Converters;

public class BoolArrayToFwRuleCategoriesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType != typeof(string)) return null;
        const int expectedLength = 3;
        var fallback = GetTranslation(Enum.GetName(NetworkProfiles.NotConfigured), false);
        if (value is not bool[] { Length: expectedLength } boolArray)
            return fallback;
        var result = string.Empty;
        var numSelected = boolArray.CountAny(true);
        switch (numSelected)
        {
            case 0:
                return fallback;
            case < 2:
                return GetTranslation(Enum.GetName(typeof(NetworkProfiles),
                    Array.FindIndex(boolArray, b => b)), false);
        }

        if (boolArray.All(b => b))
            return Strings.All;

        for (var i = 0; i < expectedLength; i++)
        {
            if (boolArray[i])
                result += $", {GetTranslation(Enum.GetName(typeof(NetworkProfiles), i), true)}";
        }

        return result[2..];

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static string GetTranslation(string key, bool trimmed)
    {
        return Strings.ResourceManager.GetString(trimmed ? $"{key}_Short3" : key, Strings.Culture);
    }
}