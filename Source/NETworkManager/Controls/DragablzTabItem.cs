using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dragablz;
using NETworkManager.Utilities;
using NETworkManager.ViewModels;

namespace NETworkManager.Controls;

public class DragablzTabItem : ViewModelBase
{
    /// <summary>
    ///     Creates a new instance of the <see cref="DragablzTabItem" /> class.
    /// </summary>
    /// <param name="header">Header of the tab.</param>
    /// <param name="view">View of the tab.</param>
    /// <param name="id">Id of the tab.</param>
    public DragablzTabItem(string header, UserControl view, Guid id)
    {
        Header = header;
        View = view;
        Id = id;
    }

    /// <summary>
    ///     Header of the tab.
    /// </summary>
    public string Header
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     View of the tab.
    /// </summary>
    public UserControl View { get; }

    /// <summary>
    ///     Id of the tab.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     Method to set the <see cref="Header" /> of a <see cref="DragablzTabItem" /> based on the <see cref="Id" />.
    ///     Searches all windows (main window and dragged out tab windows), not only the active one - the tab can be in
    ///     a window that is not active.
    /// </summary>
    /// <param name="tabId">Id of the tab to set the header.</param>
    /// <param name="header">New header to set.</param>
    public static void SetTabHeader(Guid tabId, string header)
    {
        var tabItem = Application.Current.Windows.OfType<Window>()
            .SelectMany(GetTabItems)
            .FirstOrDefault(x => x.Id == tabId);

        if (tabItem != null)
            tabItem.Header = header;
    }

    /// <summary>
    ///     All tab items of a window (main window or tab window): the items of all <see cref="TabablzControl" />s in
    ///     it, incl. split layout branches. Uses the visual tree, not <see cref="TabablzControl.GetLoadedInstances" />:
    ///     a branch that was just created is not loaded yet, but its tabs must be found (e.g. closed with the window).
    ///     Lazy (stops at the first match of e.g. <see cref="SetTabHeader" />) - call <c>ToList()</c> before closing tabs
    ///     while iterating.
    /// </summary>
    /// <param name="window">Window to search.</param>
    public static IEnumerable<DragablzTabItem> GetTabItems(DependencyObject window)
    {
        return VisualTreeHelper.FindVisualChildren<TabablzControl>(window)
            .SelectMany(tabablzControl => tabablzControl.Items.OfType<DragablzTabItem>());
    }

    /// <summary>
    ///     Focuses the embedded window (e.g. PuTTY, PowerShell) of the selected tab in the first
    ///     <see cref="TabablzControl" /> of the window that has tabs.
    /// </summary>
    /// <param name="window">Window to search.</param>
    public static void FocusSelectedEmbeddedWindow(DependencyObject window)
    {
        var tabablzControl = VisualTreeHelper.FindVisualChildren<TabablzControl>(window)
            .FirstOrDefault(x => x.Items.Count > 0);

        ((tabablzControl?.SelectedItem as DragablzTabItem)?.View as IEmbeddedWindow)?.FocusEmbeddedWindow();
    }
}