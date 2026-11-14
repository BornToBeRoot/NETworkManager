using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NETworkManager.Controls;

/// <summary>
///     Notice below a list that a tool needs elevated rights (e.g. read-only mode of the Firewall): one row with a
///     shield, the <see cref="Text" /> and the button "Restart as Administrator" (<see cref="Command" />) on the right.
///     The look (template) is defined by the style <c>DefaultAdminNotice</c> of the application (TextBlockStyles.xaml,
///     implicit style).
/// </summary>
public class AdminNotice : Control
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text),
        typeof(string), typeof(AdminNotice), new PropertyMetadata(null));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command),
        typeof(ICommand), typeof(AdminNotice), new PropertyMetadata(null));

    /// <summary>
    ///     Message (e.g. why the tool is read-only).
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    ///     Command of the button "Restart as Administrator".
    /// </summary>
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
}
