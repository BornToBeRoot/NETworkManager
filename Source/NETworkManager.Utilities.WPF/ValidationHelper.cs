using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Utilities.WPF;

/// <summary>
/// This allows propagating validation errors to a ViewModel allowing style changes bound
/// to the view, e.g., red border on a DataGridRow.
/// </summary>
/// <remarks>
/// Class is AI-generated. See FirewallRuleGrid control for usage.
/// </remarks>
public static class ValidationHelper
{
    // This property acts as a bridge. We can write to it from a Style Trigger, 
    // and it can push that value to the ViewModel via OneWayToSource binding.
    public static readonly DependencyProperty HasErrorProperty = DependencyProperty.RegisterAttached(
        "HasError",
        typeof(bool),
        typeof(ValidationHelper),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static void SetHasError(DependencyObject element, bool value) => element.SetValue(HasErrorProperty, value);

    public static bool GetHasError(DependencyObject element) => (bool)element.GetValue(HasErrorProperty);
    
    // Observe validation errors directly. Required unless NotifyOnValidationErrors is set.
    public static readonly DependencyProperty ObserveValidationProperty = DependencyProperty.RegisterAttached(
        "ObserveValidation",
        typeof(bool),
        typeof(ValidationHelper),
        new PropertyMetadata(false, OnObserveValidationChanged));

    public static void SetObserveValidation(DependencyObject element, bool value) => element.SetValue(ObserveValidationProperty, value);

    public static bool GetObserveValidation(DependencyObject element) => (bool)element.GetValue(ObserveValidationProperty);

    private static void OnObserveValidationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        if ((bool)e.NewValue)
        {
            // Listen to the Validation.HasError property changes directly
            var descriptor = DependencyPropertyDescriptor.FromProperty(Validation.HasErrorProperty, typeof(FrameworkElement));
            descriptor.AddValueChanged(element, OnValidationHasErrorChanged);
            
            // Initial sync
            SetHasError(element, Validation.GetHasError(element));
        }
        else
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(Validation.HasErrorProperty, typeof(FrameworkElement));
            descriptor.RemoveValueChanged(element, OnValidationHasErrorChanged);
        }
    }

    private static void OnValidationHasErrorChanged(object sender, EventArgs e)
    {
        if (sender is DependencyObject d)
        {
            SetHasError(d, Validation.GetHasError(d));
        }
    }
}