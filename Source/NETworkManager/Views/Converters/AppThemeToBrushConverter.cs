using MahApps.Metro;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace NETworkManager.Views.Converters
{
    public sealed class AppThemeToBrushConverter : IValueConverter
    {
        /* Convert an MahApps.Metro.Accent (from wpf/xaml-binding) to a Brush to fill rectangle with color in a ComboBox */
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            return ThemeManager.AppThemes.First(x => x.Name == ((AppTheme)System.Convert.ChangeType(value, typeof(AppTheme))).Name).Resources["WindowBackgroundBrush"] as Brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
