using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace NETworkManager.Utilities.WPF
{
    public class PasswordBoxBindingBehavior : Behavior<PasswordBox>
    {
        /// <summary>
        /// 
        /// </summary>
        protected override void OnAttached()
        {
            AssociatedObject.PasswordChanged += OnPasswordBoxValueChanged;
        }

        public SecureString Password
        {
            get => (SecureString)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password", typeof(SecureString), typeof(PasswordBoxBindingBehavior), new PropertyMetadata(null));
        
        private void OnPasswordBoxValueChanged(object sender, RoutedEventArgs e)
        {
            var binding = BindingOperations.GetBindingExpression(this, PasswordProperty);

            if (binding == null)
                return;

            var property = binding.DataItem.GetType().GetProperty(binding.ParentBinding.Path.Path);

            if (property != null)
                property.SetValue(binding.DataItem, AssociatedObject.SecurePassword, null);
        }
    }
}
