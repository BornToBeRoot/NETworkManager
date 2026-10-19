using System.Windows;
using System.Windows.Controls;

namespace NETworkManager.Utilities.WPF;

/// <summary>
/// Attached-property helper to show a hint (e.g. "requires elevated rights") as tooltip on a control,
/// which is disabled because the application is missing a permission (e.g. not running as administrator).
/// The tooltip is also shown while the control is disabled and only if <see cref="HasPermissionProperty"/>
/// is <see langword="false"/>. An existing tooltip of the control is never overwritten.
/// </summary>
public static class PermissionToolTipHelper
{
    /// <summary>
    /// Resource key of the tooltip style (limits the width and wraps long text).
    /// Defined in "Resources/Styles/ToolTipStyles.xaml" of the application.
    /// </summary>
    private const string ToolTipStyleKey = "WrapToolTip";

    /// <summary>
    /// Text of the tooltip, which is shown if the permission is missing.
    /// </summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(PermissionToolTipHelper),
            new PropertyMetadata(null, OnChanged));

    public static void SetText(FrameworkElement element, string value) =>
        element.SetValue(TextProperty, value);

    public static string GetText(FrameworkElement element) =>
        (string)element.GetValue(TextProperty);

    /// <summary>
    /// Indicates whether the permission is available (e.g. running as administrator).
    /// Defaults to <see langword="true"/>, so no tooltip is shown until the binding is resolved.
    /// </summary>
    public static readonly DependencyProperty HasPermissionProperty =
        DependencyProperty.RegisterAttached(
            "HasPermission",
            typeof(bool),
            typeof(PermissionToolTipHelper),
            new PropertyMetadata(true, OnChanged));

    public static void SetHasPermission(FrameworkElement element, bool value) =>
        element.SetValue(HasPermissionProperty, value);

    public static bool GetHasPermission(FrameworkElement element) =>
        (bool)element.GetValue(HasPermissionProperty);

    /// <summary>
    /// Indicates whether the tooltip was set by this helper (to only clear our own tooltip).
    /// </summary>
    private static readonly DependencyProperty IsAppliedProperty =
        DependencyProperty.RegisterAttached(
            "IsApplied",
            typeof(bool),
            typeof(PermissionToolTipHelper),
            new PropertyMetadata(false));

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        var text = GetText(element);
        var isApplied = (bool)element.GetValue(IsAppliedProperty);

        if (!GetHasPermission(element) && !string.IsNullOrEmpty(text))
        {
            // Don't overwrite a tooltip that was set on the control itself
            if (!isApplied && element.ToolTip != null)
                return;

            var toolTip = new ToolTip { Content = text };
            toolTip.SetResourceReference(FrameworkElement.StyleProperty, ToolTipStyleKey);

            element.ToolTip = toolTip;
            ToolTipService.SetShowOnDisabled(element, true);
            element.SetValue(IsAppliedProperty, true);
        }
        else if (isApplied)
        {
            element.ClearValue(FrameworkElement.ToolTipProperty);
            element.ClearValue(ToolTipService.ShowOnDisabledProperty);
            element.SetValue(IsAppliedProperty, false);
        }
    }
}
