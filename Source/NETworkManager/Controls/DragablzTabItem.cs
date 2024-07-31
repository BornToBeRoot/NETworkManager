using System;
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
    ///     Private field for the <see cref="Header" /> property.
    /// </summary>
    private string _header;

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
        get => _header;
        private set
        {
            if (value == _header)
                return;

            _header = value;
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
    ///     Method to set the <see cref="Header" /> of a <see cref="DragablzTabItem" /> based on the <see cref="Id" />
    ///     in the current <see cref="Window" /> by finding the tab item in all <see cref="TabablzControl" />`s
    ///     via the <see cref="VisualTreeHelper" />.
    /// </summary>
    /// <param name="tabId">Id of the tab to set the header.</param>
    /// <param name="header">New header to set.</param>
    public static void SetTabHeader(Guid tabId, string header)
    {
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        if (window == null)
            return;

        // Find all TabablzControl in the active window
        foreach (var tabablzControl in VisualTreeHelper.FindVisualChildren<TabablzControl>(window))
        {
            var tabItem = tabablzControl.Items.OfType<DragablzTabItem>().FirstOrDefault(x => x.Id == tabId);

            if (tabItem == null)
                continue;

            tabItem.Header = header;
            break;
        }
    }
}