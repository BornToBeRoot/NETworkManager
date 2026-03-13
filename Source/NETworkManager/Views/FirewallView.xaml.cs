using System.Windows;
using System.Windows.Controls;
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
    private readonly FirewallViewModel _viewModel;

    /// <summary>
    /// Initialize view.
    /// </summary>
    public FirewallView()
    {
        InitializeComponent();

        _viewModel = FirewallViewModel.Instance as FirewallViewModel;
        DataContext = _viewModel;
        _viewModel?.CommandExecuted += AnyButton_OnClick;
        FirewallRuleGrid.DataContext = _viewModel;
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

    /// <summary>
    /// Offload event for toggling view to the view model.
    /// </summary>
    public void OnViewHide()
    {
        _viewModel.OnViewHide();
        if (_viewModel.IsViewActive)
            FirewallRuleGrid?.RestoreRuleGridFocus();
    }

    /// <summary>
    /// Offload event for showing the view after editing settings to view model.
    /// </summary>
    public void OnViewVisible()
    {
        _viewModel.OnViewVisible();
        FirewallRuleGrid?.RestoreRuleGridFocus();
    }

    /// <summary>
    /// Set the focus to the RuleGrid when loading is finished.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FirewallView_OnLoaded(object sender, RoutedEventArgs e)
    {
        FirewallRuleGrid?.RestoreRuleGridFocus();
    }
    #endregion

    /// <summary>
    /// Set the focus to the rule grid when a button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AnyButton_OnClick(object sender, RoutedEventArgs e)
    {
        FirewallRuleGrid?.RestoreRuleGridFocus();
    }
}