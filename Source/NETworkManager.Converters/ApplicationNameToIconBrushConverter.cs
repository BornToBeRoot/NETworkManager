using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using NETworkManager.Models;

namespace NETworkManager.Converters;

/// <summary>
///     Convert <see cref="ApplicationName" /> to its icon as <see cref="VisualBrush" /> (e.g. for an OpacityMask). Can be
///     used as value of a style setter, unlike a <see cref="VisualBrush" /> with a binding in the setter, which has no
///     element to resolve the binding against.
/// </summary>
public sealed class ApplicationNameToIconBrushConverter : IValueConverter
{
    /// <summary>
    ///     Convert <see cref="ApplicationName" /> to its icon as <see cref="VisualBrush" />.
    /// </summary>
    /// <param name="value">Object from type <see cref="ApplicationName" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Icon as <see cref="VisualBrush" />.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not ApplicationName name
            ? null
            : new VisualBrush(ApplicationManager.GetIcon(name)) { Stretch = Stretch.Uniform };
    }

    /// <summary>
    ///     !!! Method not implemented !!!
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
