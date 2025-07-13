using NETworkManager.Utilities;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NETworkManager.Converters;

public sealed class ChildWindowIconToRectangleStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ChildWindowIcon icon)
            return null;

        switch (icon)
        {
            case ChildWindowIcon.Info:
                return Application.Current.FindResource("InfoImageRectangle");
            case ChildWindowIcon.Warn:
                return Application.Current.FindResource("WarnImageRectangle");
            case ChildWindowIcon.Error:
                return Application.Current.FindResource("ErrorImageRectangle");
            default:
                return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}