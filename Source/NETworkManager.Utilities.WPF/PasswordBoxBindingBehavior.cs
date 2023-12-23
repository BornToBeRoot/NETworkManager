using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

namespace NETworkManager.Utilities.WPF;

public class PasswordBoxBindingBehavior : Behavior<PasswordBox>
{
    public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password",
        typeof(SecureString), typeof(PasswordBoxBindingBehavior), new PropertyMetadata(null));

    public SecureString Password
    {
        get => (SecureString)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    /// <summary>
    /// </summary>
    protected override void OnAttached()
    {
        AssociatedObject.PasswordChanged += OnPasswordBoxValueChanged;
    }

    private void OnPasswordBoxValueChanged(object sender, RoutedEventArgs e)
    {
        var binding = BindingOperations.GetBindingExpression(this, PasswordProperty);

        if (binding == null)
            return;

        var property = binding.DataItem.GetType().GetProperty(binding.ParentBinding.Path.Path);

        property?.SetValue(binding.DataItem, AssociatedObject.SecurePassword, null);
    }
}