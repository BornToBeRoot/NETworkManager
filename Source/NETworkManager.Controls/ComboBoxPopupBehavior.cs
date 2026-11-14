using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NETworkManager.Controls;

/// <summary>
/// Attached property that shows the drop-down of a <see cref="ComboBox"/> like the context menu: rounded corners, a
/// soft shadow, a small padding around the items and a small gap below the ComboBox (above it if there is no space
/// below).
///
/// The drop-down is part of the template of MahApps (<c>PART_Popup</c> with the <c>PopupBorder</c>, 2.4.11). It is
/// changed when the drop-down is opened instead of copying the whole ComboBox template (~400 lines, which would have
/// to be kept in sync with MahApps). If the parts are not found (e.g. another template), nothing is changed.
/// </summary>
public static class ComboBoxPopupBehavior
{
    /// <summary>
    /// Space for the shadow on the right and at the bottom (like the context menu).
    /// </summary>
    private const double ShadowSize = 6;

    /// <summary>
    /// Gap between the ComboBox and the drop-down.
    /// </summary>
    private const double Gap = 2;

    public static readonly DependencyProperty IsRoundedProperty =
        DependencyProperty.RegisterAttached(
            "IsRounded",
            typeof(bool),
            typeof(ComboBoxPopupBehavior),
            new PropertyMetadata(false, OnIsRoundedChanged));

    public static void SetIsRounded(UIElement element, bool value) =>
        element.SetValue(IsRoundedProperty, value);

    public static bool GetIsRounded(UIElement element) =>
        (bool)element.GetValue(IsRoundedProperty);

    /// <summary>
    /// Shadow border added behind the <c>PopupBorder</c> (the shadow is not set on the <c>PopupBorder</c> itself,
    /// an effect on it would render the text of the items without ClearType).
    /// </summary>
    private static readonly DependencyProperty ShadowBorderProperty =
        DependencyProperty.RegisterAttached(
            "ShadowBorder",
            typeof(Border),
            typeof(ComboBoxPopupBehavior),
            new PropertyMetadata(null));

    private static void OnIsRoundedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox comboBox)
            return;

        if ((bool)e.NewValue)
            comboBox.DropDownOpened += ComboBox_DropDownOpened;
        else
            comboBox.DropDownOpened -= ComboBox_DropDownOpened;
    }

    private static void ComboBox_DropDownOpened(object sender, System.EventArgs e)
    {
        if (sender is not ComboBox comboBox || comboBox.Template == null)
            return;

        if (comboBox.Template.FindName("PART_Popup", comboBox) is not Popup popup ||
            comboBox.Template.FindName("PopupBorder", comboBox) is not Border popupBorder ||
            popupBorder.Parent is not Grid popupGrid)
            return;

        // At least as wide as the ComboBox (MahApps binds MinWidth to its width), plus the space for the shadow.
        // Set on every opening, the width of the ComboBox can change.
        popupGrid.MinWidth = comboBox.ActualWidth + ShadowSize;

        // Already changed (the template parts are kept until the template changes)
        if (popupGrid.GetValue(ShadowBorderProperty) != null)
            return;

        var cornerRadius = comboBox.TryFindResource("CornerRadius.Control") is CornerRadius radius
            ? radius
            : new CornerRadius(4);

        var shadowMargin = new Thickness(0, 0, ShadowSize, ShadowSize);

        var shadowBorder = new Border
        {
            CornerRadius = cornerRadius,
            Margin = shadowMargin,
            IsHitTestVisible = false
        };
        shadowBorder.SetResourceReference(Border.BackgroundProperty, "MahApps.Brushes.ThemeBackground");
        shadowBorder.SetResourceReference(UIElement.EffectProperty, "MahApps.DropShadowEffect.Menu");

        popupGrid.Children.Insert(0, shadowBorder);
        popupGrid.SetValue(ShadowBorderProperty, shadowBorder);

        popupBorder.CornerRadius = cornerRadius;
        popupBorder.Margin = shadowMargin;
        popupBorder.Padding = new Thickness(3);
        popupBorder.SetResourceReference(Border.BorderBrushProperty, "Brushes.Surface.Border");

        // Small gap below the ComboBox, or above it if there is no space below (near the bottom of the screen). With
        // Placement Bottom and a VerticalOffset the flipped drop-down would overlap the ComboBox and the space for the
        // shadow would be between them. The popup of MahApps already allows transparency for the rounded corners.
        popup.VerticalOffset = 0;
        popup.Placement = PlacementMode.Custom;
        popup.CustomPopupPlacementCallback = (popupSize, targetSize, _) =>
        {
            // The sizes and the returned positions are in device pixels
            var scale = PresentationSource.FromVisual(comboBox)?.CompositionTarget?.TransformToDevice.M22 ?? 1;

            return
            [
                new CustomPopupPlacement(new Point(0, targetSize.Height + Gap * scale), PopupPrimaryAxis.Horizontal),
                // Above: the bottom of the box (without the space for the shadow) a gap above the ComboBox
                new CustomPopupPlacement(new Point(0, -(popupSize.Height - ShadowSize * scale) - Gap * scale),
                    PopupPrimaryAxis.Horizontal)
            ];
        };
    }
}
