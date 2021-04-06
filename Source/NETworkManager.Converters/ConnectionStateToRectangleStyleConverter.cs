using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters
{
    public sealed class ConnectionStateToRectangleStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is ConnectionState state)
            {
                switch (state)
                {
                    case ConnectionState.None:
                        break;
                    case ConnectionState.OK:
                        return Application.Current.Resources["CheckRectangle"] as Style;
                    case ConnectionState.Warning:
                        return Application.Current.Resources["AlertRectangle"] as Style;
                    case ConnectionState.Critical:
                        return Application.Current.Resources["ErrorRectangle"] as Style;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return Application.Current.Resources["HiddenRectangle"] as Style;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
