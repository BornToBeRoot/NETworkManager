using System.Windows;

namespace NETworkManager.Utilities.WPF;

/// <summary>
/// Provides a binding proxy class for enabling bindings to data contexts that are not directly
/// accessible within a XAML element's logical or visual tree.
/// </summary>
/// <remarks>
/// This class inherits from <see cref="Freezable"/> and uses a dependency property to hold the
/// data context. It is particularly useful in scenarios where bindings are required in data templates
/// or styles, and the desired data context is otherwise inaccessible.
/// </remarks>
public class BindingProxy : Freezable
{
    /// <summary>
    /// Dependency property used to hold a generic object.
    /// This property allows data binding scenarios where a proxy
    /// is required to transfer data between binding contexts.
    /// </summary>
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy));

    /// <summary>
    /// Gets or sets the data object used for binding in WPF applications.
    /// </summary>
    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="BindingProxy"/> class.
    /// </summary>
    /// <returns>
    /// A new instance of the <see cref="BindingProxy"/> class.
    /// </returns>
    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }
}
