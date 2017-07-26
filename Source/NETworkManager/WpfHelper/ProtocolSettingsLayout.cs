using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NETworkManager.WpfHelper
{
    public class ProtocolSettingsLayout
    {
        public static readonly DependencyProperty MVVMHasErrorProperty = DependencyProperty.RegisterAttached("MVVMHasError", typeof(bool), typeof(ProtocolSettingsLayout), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, CoerceMVVMHasError));

        public static bool GetMVVMHasError(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(MVVMHasErrorProperty);
        }

        public static void SetMVVMHasError(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(MVVMHasErrorProperty, value);
        }

        private static object CoerceMVVMHasError(DependencyObject dependencyObject, Object baseValue)
        {
            bool hasError = (bool)baseValue;

            if (BindingOperations.IsDataBound(dependencyObject, MVVMHasErrorProperty))
            {
                if (GetHasErrorDescriptor(dependencyObject) == null)
                {
                    DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(Validation.HasErrorProperty, dependencyObject.GetType());
                    descriptor.AddValueChanged(dependencyObject, OnHasErrorChanged);
                    SetHasErrorDescriptor(dependencyObject, descriptor);
                    hasError = Validation.GetHasError(dependencyObject);
                }
            }
            else
            {
                if (GetHasErrorDescriptor(dependencyObject) != null)
                {
                    DependencyPropertyDescriptor descriptor = GetHasErrorDescriptor(dependencyObject);
                    descriptor.RemoveValueChanged(dependencyObject, OnHasErrorChanged);
                    SetHasErrorDescriptor(dependencyObject, null);
                }
            }

            return hasError;
        }

        private static readonly DependencyProperty HasErrorDescriptorProperty = DependencyProperty.RegisterAttached("HasErrorDescriptor", typeof(DependencyPropertyDescriptor), typeof(ProtocolSettingsLayout));

        private static DependencyPropertyDescriptor GetHasErrorDescriptor(DependencyObject dependencyObject)
        {
            object descriptor = dependencyObject.GetValue(HasErrorDescriptorProperty);
            return descriptor as DependencyPropertyDescriptor;
        }

        private static void OnHasErrorChanged(object sender, EventArgs e)
        {
            if (sender is DependencyObject dependencyObject)
            {
                dependencyObject.SetValue(MVVMHasErrorProperty, dependencyObject.GetValue(Validation.HasErrorProperty));
            }
        }

        private static void SetHasErrorDescriptor(DependencyObject dependencyObject, DependencyPropertyDescriptor descriptor)
        {
            dependencyObject.SetValue(HasErrorDescriptorProperty, descriptor);
        }
    }
}
