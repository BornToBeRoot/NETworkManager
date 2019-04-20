using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Models.Settings;

namespace NETworkManager.Converters
{
    public sealed class RemoteDesktopKeyboardHookModeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return LocalizationManager.TranslateRemoteDesktopKeyboardHookMode(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
