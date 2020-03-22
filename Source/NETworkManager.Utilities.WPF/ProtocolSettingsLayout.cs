using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NETworkManager.Utilities.WPF
{
    /// <summary>
    /// 
    /// </summary>
    public class ProtocolSettingsLayout
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty MVVMHasErrorProperty = DependencyProperty.RegisterAttached("MVVMHasError", typeof(bool), typeof(ProtocolSettingsLayout), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, CoerceMVVMHasError));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        public static bool GetMVVMHasError(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(MVVMHasErrorProperty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="value"></param>
        public static void SetMVVMHasError(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(MVVMHasErrorProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="baseValue"></param>
        /// <returns></returns>
        private static object CoerceMVVMHasError(DependencyObject dependencyObject, object baseValue)
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

        /// <summary>
        /// 
        /// </summary>
        private static readonly DependencyProperty HasErrorDescriptorProperty = DependencyProperty.RegisterAttached("HasErrorDescriptor", typeof(DependencyPropertyDescriptor), typeof(ProtocolSettingsLayout));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        private static DependencyPropertyDescriptor GetHasErrorDescriptor(DependencyObject dependencyObject)
        {
            object descriptor = dependencyObject.GetValue(HasErrorDescriptorProperty);
            return descriptor as DependencyPropertyDescriptor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnHasErrorChanged(object sender, EventArgs e)
        {
            if (sender is DependencyObject dependencyObject)
            {
                dependencyObject.SetValue(MVVMHasErrorProperty, dependencyObject.GetValue(Validation.HasErrorProperty));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="descriptor"></param>
        private static void SetHasErrorDescriptor(DependencyObject dependencyObject, DependencyPropertyDescriptor descriptor)
        {
            dependencyObject.SetValue(HasErrorDescriptorProperty, descriptor);
        }
    }
}
