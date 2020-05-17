using MahApps.Metro;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace NETworkManager.Converters
{
    ///// <summary>
    ///// Convert <see cref="Accent"/> to <see cref="Brush"/> or wise versa.
    ///// </summary>
    //public sealed class AccentToBrushConverter : IValueConverter
    //{        
    //    /// <summary>
    //    /// Convert <see cref="Accent"/> to <see cref="Brush"/>. 
    //    /// </summary>
    //    /// <param name="value">Object from type <see cref="Accent"/>.</param>
    //    /// <param name="targetType"></param>
    //    /// <param name="parameter"></param>
    //    /// <param name="culture"></param>
    //    /// <returns>Converted <see cref="Brush"/>.</returns>
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return ThemeManager.Accents.First(x => x.Name == ((Accent)System.Convert.ChangeType(value, typeof(Accent))).Name).Resources["AccentColorBrush"] as Brush;
    //    }

    //    /// <summary>
    //    /// !!! Method not implemented !!!
    //    /// </summary>
    //    /// <param name="value"></param>
    //    /// <param name="targetType"></param>
    //    /// <param name="parameter"></param>
    //    /// <param name="culture"></param>
    //    /// <returns></returns>
    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
