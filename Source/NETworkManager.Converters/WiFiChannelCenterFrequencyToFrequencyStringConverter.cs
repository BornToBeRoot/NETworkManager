﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;

public sealed class WiFiChannelCenterFrequencyToFrequencyStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not double frequency ? "-/-" : $"{frequency} GHz";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}