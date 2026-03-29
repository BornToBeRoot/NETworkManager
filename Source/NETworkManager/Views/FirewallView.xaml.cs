using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

/// <summary>
/// Code-behind for the firewall view.
/// </summary>
public partial class FirewallView
{
    /// <summary>
    /// View model for the view.
    /// </summary>
    private readonly FirewallViewModel _viewModel = new();

    /// <summary>
    /// Initialize view.
    /// </summary>
    public FirewallView()
    {
        InitializeComponent();
        DataContext = _viewModel;
    }

    #region Events
    /// <summary>
    /// Set data context for menus.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = _viewModel;
    }
    
    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            _viewModel.ApplyProfileCommand.Execute(null);
    }

    /// <summary>
    /// Offload event for toggling view to the view model.
    /// </summary>
    public void OnViewHide()
    {
        _viewModel.OnViewHide();
    }

    /// <summary>
    /// Offload event for showing the view after editing settings to view model.
    /// </summary>
    public void OnViewVisible()
    {
        _viewModel.OnViewVisible();
    }
    #endregion
}
