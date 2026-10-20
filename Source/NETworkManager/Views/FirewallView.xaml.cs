using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
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

        _viewModel.OnContextMenuOpened();
    }

    /// <summary>
    /// Toggles the row details visibility when the expand/collapse chevron is clicked.
    /// </summary>
    private void ExpandRowDetails_OnClick(object sender, RoutedEventArgs e)
    {
        for (var visual = sender as Visual; visual != null; visual = VisualTreeHelper.GetParent(visual) as Visual)
        {
            if (visual is not DataGridRow row)
                continue;

            row.DetailsVisibility =
                row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            break;
        }
    }

    /// <summary>
    /// Opens the edit dialog for the rule when a row is double-clicked (if the rule can be edited).
    /// Double-clicks on buttons / checkboxes (e.g. the expand/collapse chevron) and in the row details
    /// (e.g. to select text) are ignored.
    /// </summary>
    private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        for (var element = e.OriginalSource as DependencyObject; element != null && element != sender;
             element = element is Visual ? VisualTreeHelper.GetParent(element) : LogicalTreeHelper.GetParent(element))
        {
            if (element is ButtonBase or DataGridDetailsPresenter)
                return;
        }

        if (_viewModel.EditRuleCommand.CanExecute(null))
            _viewModel.EditRuleCommand.Execute(null);
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
