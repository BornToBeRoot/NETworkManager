using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Controls;

/// <summary>
///     Status message of a tool (e.g. below the input: an error, "canceled by user", "magic packet sent"): an icon and
///     the text in the color of the <see cref="Level" /> - info gray (<see cref="StatusLevel.None" />), success green,
///     warning yellow, error red. The look is defined by the style <c>DefaultStatusMessageText</c> of the application
///     (TextBlockStyles.xaml, implicit style).
/// </summary>
public class StatusMessageText : Control
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text),
        typeof(string), typeof(StatusMessageText), new PropertyMetadata(null));

    public static readonly DependencyProperty LevelProperty = DependencyProperty.Register(nameof(Level),
        typeof(StatusLevel), typeof(StatusMessageText), new PropertyMetadata(StatusLevel.None));

    /// <summary>
    ///     Message (can have several lines).
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    ///     Level of the message (icon and color).
    /// </summary>
    public StatusLevel Level
    {
        get => (StatusLevel)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }
}
