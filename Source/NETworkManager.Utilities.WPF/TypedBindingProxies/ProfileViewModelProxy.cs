using System.Windows;
using NETworkManager.Interfaces.ViewModels;

namespace NETworkManager.Utilities.WPF.TypedBindingProxies;

/// <summary>
/// Binding proxy for <see cref="IProfileViewModel"/>s.
/// </summary>
public class ProfileViewModelProxy : Freezable
{
    /// <summary>
    /// Dependency property used to hold a generic object.
    /// This property allows data binding scenarios where a proxy
    /// is required to transfer data between binding contexts.
    /// </summary>
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(IProfileViewModel), typeof(ProfileViewModelProxy));

    /// <summary>
    /// Gets or sets the data object used for binding in WPF applications.
    /// </summary>
    public IProfileViewModel Data
    {
        get => (IProfileViewModel)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// Creates a new instance of the BindingProxy class.
    /// <returns>
    /// A new instance of the BindingProxy class.
    /// </returns>
    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }
}