using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Controls;

/// <summary>
///     Message in an empty area (e.g. no rules found, restart as administrator, configure the path): a gray
///     <see cref="Icon" /> above the <see cref="Text" />. The look (template, text style) is defined by the style
///     <c>DefaultEmptyStateMessage</c> of the application (TextBlockStyles.xaml, implicit style).
/// </summary>
public class EmptyStateMessage : Control
{
    /// <summary>
    ///     Default size of the icon (empty area). Notices (e.g. restart as administrator below a list) use 24.
    /// </summary>
    public const double DefaultIconSize = 40;

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text),
        typeof(string), typeof(EmptyStateMessage), new PropertyMetadata(null));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon),
        typeof(LucideIconKind), typeof(EmptyStateMessage), new PropertyMetadata(LucideIconKind.None));

    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(nameof(IconSize),
        typeof(double), typeof(EmptyStateMessage), new PropertyMetadata(DefaultIconSize));

    /// <summary>
    ///     Message.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    ///     Icon above the message (none = only the message).
    /// </summary>
    public LucideIconKind Icon
    {
        get => (LucideIconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    ///     Size of the icon (default <see cref="DefaultIconSize" />).
    /// </summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }
}
