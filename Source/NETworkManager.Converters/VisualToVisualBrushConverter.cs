using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NETworkManager.Converters;

/// <summary>
///     Convert a <see cref="Visual" /> (e.g. an icon) to a <see cref="VisualBrush" /> (e.g. for an OpacityMask). The
///     binding is on the element, unlike a <see cref="VisualBrush" /> with a binding on <see cref="VisualBrush.Visual" />,
///     which has no element to resolve the binding against in some cases (logged as XAML binding error).
/// </summary>
public sealed class VisualToVisualBrushConverter : IValueConverter
{
    /// <summary>
    ///     Convert a <see cref="Visual" /> to a <see cref="VisualBrush" />.
    /// </summary>
    /// <param name="value">Object from type <see cref="Visual" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns><see cref="VisualBrush" /> of the visual.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not Visual visual ? null : new VisualBrush(visual) { Stretch = Stretch.Uniform };
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
