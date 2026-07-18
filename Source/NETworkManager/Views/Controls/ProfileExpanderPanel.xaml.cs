using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NETworkManager.Views.Controls;

/// <summary>
///     Reusable profile panel (search, tag filter, group list) shared by every tool view that hosts profiles.
///     Tool-specific parts (context menu, item tooltip, "run" action) are supplied by the host view through
///     dependency properties; everything else is identical across tools.
/// </summary>
public partial class ProfileExpanderPanel
{
    public static readonly DependencyProperty ProfileItemTemplateProperty = DependencyProperty.Register(
        nameof(ProfileItemTemplate), typeof(DataTemplate), typeof(ProfileExpanderPanel));

    /// <summary>
    ///     Gets or sets the item template used for a profile in the list (defines the tool-specific tooltip).
    /// </summary>
    public DataTemplate ProfileItemTemplate
    {
        get => (DataTemplate)GetValue(ProfileItemTemplateProperty);
        set => SetValue(ProfileItemTemplateProperty, value);
    }

    public static readonly DependencyProperty ProfileContextMenuExtraItemsProperty = DependencyProperty.Register(
        nameof(ProfileContextMenuExtraItems), typeof(IEnumerable), typeof(ProfileExpanderPanel));

    /// <summary>
    ///     Gets or sets the tool-specific menu items (commands/icons) shown at the top of the profile context
    ///     menu, before the always-present "Edit", "Copy as", and "Delete" items.
    /// </summary>
    public IEnumerable ProfileContextMenuExtraItems
    {
        get => (IEnumerable)GetValue(ProfileContextMenuExtraItemsProperty);
        set => SetValue(ProfileContextMenuExtraItemsProperty, value);
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        // The context menu is a separate visual root (popup) and does not inherit the DataContext of the
        // ListBoxItem it was opened from - fix it up here so its command bindings resolve against the
        // tool's view model, same as the panel's own DataContext.
        if (sender is ContextMenu menu)
            menu.DataContext = DataContext;
    }

    public static readonly DependencyProperty RunProfileCommandProperty = DependencyProperty.Register(
        nameof(RunProfileCommand), typeof(ICommand), typeof(ProfileExpanderPanel));

    /// <summary>
    ///     Gets or sets the command executed when a profile is activated (Enter key or double-click). Optional -
    ///     tools without a "run" action for profiles leave this unset.
    /// </summary>
    public ICommand RunProfileCommand
    {
        get => (ICommand)GetValue(RunProfileCommandProperty);
        set => SetValue(RunProfileCommandProperty, value);
    }

    public static readonly DependencyProperty TextBoxSearchGotFocusCommandProperty = DependencyProperty.Register(
        nameof(TextBoxSearchGotFocusCommand), typeof(ICommand), typeof(ProfileExpanderPanel));

    /// <summary>
    ///     Gets or sets the command executed when the search box receives focus. Optional - only used by tools
    ///     that pause background refreshes while the user is searching.
    /// </summary>
    public ICommand TextBoxSearchGotFocusCommand
    {
        get => (ICommand)GetValue(TextBoxSearchGotFocusCommandProperty);
        set => SetValue(TextBoxSearchGotFocusCommandProperty, value);
    }

    public static readonly DependencyProperty TextBoxSearchLostFocusCommandProperty = DependencyProperty.Register(
        nameof(TextBoxSearchLostFocusCommand), typeof(ICommand), typeof(ProfileExpanderPanel));

    /// <summary>
    ///     Gets or sets the command executed when the search box loses focus. Optional - only used by tools
    ///     that pause background refreshes while the user is searching.
    /// </summary>
    public ICommand TextBoxSearchLostFocusCommand
    {
        get => (ICommand)GetValue(TextBoxSearchLostFocusCommandProperty);
        set => SetValue(TextBoxSearchLostFocusCommandProperty, value);
    }

    public static readonly DependencyProperty ProfileContextMenuIsOpenProperty = DependencyProperty.Register(
        nameof(ProfileContextMenuIsOpen), typeof(bool), typeof(ProfileExpanderPanel),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    ///     Gets or sets a value indicating whether the profile context menu is open. Optional - only used by
    ///     tools that need to suppress other behavior (e.g. auto-focusing an embedded window) while the menu is
    ///     open.
    /// </summary>
    public bool ProfileContextMenuIsOpen
    {
        get => (bool)GetValue(ProfileContextMenuIsOpenProperty);
        set => SetValue(ProfileContextMenuIsOpenProperty, value);
    }

    public static readonly DependencyProperty ProfileFilterPopupClosedCommandProperty = DependencyProperty.Register(
        nameof(ProfileFilterPopupClosedCommand), typeof(ICommand), typeof(ProfileExpanderPanel));

    /// <summary>
    ///     Gets or sets the command executed when the tag-filter popup closes (including an implicit close, e.g.
    ///     clicking away). Optional - only used by tools that mirror the popup's open state into global state.
    /// </summary>
    public ICommand ProfileFilterPopupClosedCommand
    {
        get => (ICommand)GetValue(ProfileFilterPopupClosedCommandProperty);
        set => SetValue(ProfileFilterPopupClosedCommandProperty, value);
    }

    public ProfileExpanderPanel()
    {
        InitializeComponent();
    }

    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            RunProfileCommand?.Execute(null);
    }

    private void PopupProfileFilter_Closed(object sender, EventArgs e)
    {
        ProfileFilterPopupClosedCommand?.Execute(null);
    }
}
