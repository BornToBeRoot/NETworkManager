using System.Windows;

namespace NETworkManager.Utilities.WPF.TypedBindingProxies;

/// <summary>
/// Binding proxy for <see cref="FrameworkElement"/>s.
/// </summary>
public class FrameworkElementProxy : Freezable
{
    /// <summary>
    /// Dependency property used to hold a generic object.
    /// This property allows data binding scenarios where a proxy
    /// is required to transfer data between binding contexts.
    /// </summary>
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(FrameworkElement), typeof(FrameworkElementProxy));

    /// <summary>
    /// Gets or sets the data object used for binding in WPF applications.
    /// </summary>
    public FrameworkElement Data
    {
        get => (FrameworkElement)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// Creates a new instance of the BindingProxy class.
    /// <returns>
    /// A new instance of the BindingProxy class.
    /// </returns>
    protected override Freezable CreateInstanceCore()
    {
        return new FrameworkElementProxy();
    }
}