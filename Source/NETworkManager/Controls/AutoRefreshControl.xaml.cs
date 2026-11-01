using System.Collections;
using System.Windows;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Controls;

/// <summary>
///     Checkbox "Automatically update every" and the interval (<see cref="NETworkManager.Utilities.AutoRefreshTimeInfo" />)
///     of a tool with an auto refresh (see <see cref="NETworkManager.Utilities.WPF.AutoRefreshTimer" />).
/// </summary>
public partial class AutoRefreshControl
{
    /// <summary>
    ///     Identifies the <see cref="IsChecked" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(nameof(IsChecked),
        typeof(bool), typeof(AutoRefreshControl),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    ///     Identifies the <see cref="ItemsSource" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource),
        typeof(IEnumerable), typeof(AutoRefreshControl), new PropertyMetadata(null));

    /// <summary>
    ///     Identifies the <see cref="SelectedItem" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(AutoRefreshControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    ///     Identifies the <see cref="Text" /> dependency property.
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text),
        typeof(string), typeof(AutoRefreshControl), new PropertyMetadata(Strings.AutomaticallyUpdateEvery));

    public AutoRefreshControl()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the auto refresh is enabled (two-way by default).
    /// </summary>
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>
    ///     Gets or sets the intervals to choose from.
    /// </summary>
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    ///     Gets or sets the selected interval (two-way by default).
    /// </summary>
    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    ///     Gets or sets the text of the checkbox (default "Automatically update every").
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}
