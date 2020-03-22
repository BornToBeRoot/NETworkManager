using MahApps.Metro;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace NETworkManager.Converters
{
    public sealed class AccentToBrushConverter : IValueConverter
    {
        /* Convert an MahApps.Metro.Accent (from wpf/xaml-binding) to a Brush to fill rectangle with color in a ComboBox */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ThemeManager.Accents.First(x => x.Name == ((Accent)System.Convert.ChangeType(value, typeof(Accent))).Name).Resources["AccentColorBrush"] as Brush;
        }

        /// <summary>
        /// Method to convert back is not implemented!
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
