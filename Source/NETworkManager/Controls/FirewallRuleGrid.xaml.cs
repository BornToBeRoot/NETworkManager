using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using NETworkManager.Converters;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Firewall;
using NETworkManager.ViewModels;

namespace NETworkManager.Controls;

/// <summary>
/// Code-behind for the firewall rule grid.
/// </summary>
public partial class FirewallRuleGrid
{
    /// <summary>
    /// Mutex to prevent spamming the history for every rule.
    /// </summary>
    /// <remarks>
    /// Every rule's local and remote port combo boxes are registered to the
    /// <see cref="ComboBoxPorts_OnLostFocus"/> method. So, if one rule changes its
    /// port values, every rule's combo boxes will trigger the event. Preventing this
    /// with the following static variables. 
    /// </remarks>
    private static readonly Mutex LocalPortsAdding = new();
    /// <summary>
    /// Last local port string, which was added.
    /// </summary>
    private static string _lastLocalPortsString = string.Empty;
    /// <summary>
    /// Mutex for remote ports being added to the history.
    /// </summary>
    private static readonly Mutex RemotePortsAdding = new();
    /// <summary>
    /// Last remote port string, which was added.
    /// </summary>
    private static string _lastRemotePortsString = string.Empty;
    
    /// <summary>
    /// List of TextBlock content to ignore on double clicks.
    /// </summary>
    private static readonly string[] IgnoredTextBlocks = [ Strings.Domain, Strings.Private, Strings.Public ];
    
    public FirewallRuleGrid()
    {
        InitializeComponent();
        RestoreRuleGridFocus();
    }
    
    #region Events
    /// <summary>
    /// Handle new valid ports and add them to the history.
    /// </summary>
    /// <param name="sender">The combo box. Not necessarily the one with the change.</param>
    /// <param name="e">Event arguments. Unused.</param>
    private void ComboBoxPorts_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not ComboBox comboBox)
            return;
        if (string.IsNullOrWhiteSpace(comboBox.Text))
            return;
        if (Validation.GetHasError(comboBox))
            return;
        var portType = (FirewallPortLocation)comboBox.Tag;
        if (comboBox.DataContext is not FirewallRuleViewModel dataContext)
            return;
        var converter = new PortRangeToPortSpecificationConverter();
        var structuredData = converter.Convert(comboBox.Text, typeof(List<FirewallPortSpecification>), null, null);
        if (structuredData is not List<FirewallPortSpecification> list)
            return;
        string formattedText = converter.ConvertBack(list, typeof(string), null, null) as string;
        if (string.IsNullOrWhiteSpace(formattedText))
            return;
        
        // Direct visual update: Find the internal TextBox and force formatting the text.
        // This bypasses ComboBox.Text property synchronization issues during LostFocus.
        if (GetVisualChild<TextBox>(comboBox) is { } textBox)
        {
            // Only update if visually different to avoid cursor/selection side effects
            if (textBox.Text != formattedText)
                textBox.Text = formattedText;
        }
        switch (portType)
        {
            case FirewallPortLocation.LocalPorts:
                if (_lastLocalPortsString == formattedText)
                    return;

                try
                {
                    LocalPortsAdding?.WaitOne();
                    _lastLocalPortsString = formattedText;
                    dataContext.AddPortsToHistory(formattedText, portType);
                }
                finally
                {
                    LocalPortsAdding?.ReleaseMutex();
                }
                break;
            case FirewallPortLocation.RemotePorts:
                if (_lastRemotePortsString == formattedText)
                    return;

                try
                {
                    RemotePortsAdding?.WaitOne();
                    _lastRemotePortsString = formattedText;
                    dataContext.AddPortsToHistory(formattedText, portType);
                }
                finally
                {
                    RemotePortsAdding?.ReleaseMutex();
                }
                break;
        }
    }
    
    /// <summary>
    /// Toggle row visibility.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not Button)
            return;
        // Get the row from the sender
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
    /// Collapse and open rule details by double clicking.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Row_OnDoubleClick(object sender, RoutedEventArgs e)
    {
        // Avoid collision with ButtonBase behavior
        if (e.OriginalSource is Button or Rectangle)
            return;
        // Get the row from the sender
        for (var visual = sender as Visual; visual != null; visual = VisualTreeHelper.GetParent(visual) as Visual)
        {
            if (visual is not DataGridRow row)
                continue;

            if (row.DetailsVisibility is Visibility.Visible
                && e.OriginalSource is not Border and not TextBlock and not CheckBox)
                return;
            if (e.OriginalSource is TextBlock textBlock && IgnoredTextBlocks.Contains(textBlock.DataContext as string))
                return;
            row.DetailsVisibility =
                row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            break;
        }
    }

    
    /// <summary>
    /// Set the data context of the context menu.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu)
            menu.DataContext = DataContext;
    }
    
    /// <summary>
    /// Keyboard control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DataGridRow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Only handle if the focus is on the cell itself (navigation mode),
        // to avoid interfering with editing (e.g., inside a TextBox).
        if (e.OriginalSource is TextBox or CheckBox or ComboBox)
            return;

        if (sender is not MultiSelectDataGrid grid)
            return;
        
        if (DataContext is not FirewallViewModel dataContext)
            return;

        DataGridRow row = null;
        if (grid.SelectedIndex > -1 && e.OriginalSource is DependencyObject source)
            row = ItemsControl.ContainerFromElement(grid, source) as DataGridRow;

        switch (e.Key)
        {
            case Key.A when Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl):
                dataContext.ApplyConfigurationCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.D when Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl):
            case Key.Delete:
                int index = RuleGrid.SelectedIndex;
                dataContext.DeleteRulesCommand.Execute(null);
                if (grid.HasItems)
                {
                    // Select the same index or the last item if we deleted the end
                    index = Math.Min(index, grid.Items.Count - 1);
                    if (index < 0)
                        index = 0;
                    grid.SelectedIndex = index;
                    grid.Focus();
                }

                e.Handled = true;
                break;
            case Key.C when (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                            && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                            && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)):
                FirewallViewModel.DeleteWindowsRulesCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.C when (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                            && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)):
                dataContext.DeleteAllRulesCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.N when Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl):
                dataContext.AddRuleCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.Right when row?.DetailsVisibility == Visibility.Collapsed:
                row.DetailsVisibility = Visibility.Visible;
                e.Handled = true;
                break;
            case Key.Left when row?.DetailsVisibility == Visibility.Visible:
                row.DetailsVisibility = Visibility.Collapsed;
                e.Handled = true;
                break;
            // If nothing is selected, start from the bottom or top row.
            case Key.Down:
            case Key.Up:
                if (!grid.IsKeyboardFocused)
                    return;
                if (row is null && grid.HasItems)
                {
                    if (grid.SelectedIndex is -1)
                        index = e.Key is Key.Down ? 0 : grid.Items.Count - 1;
                    else
                        index = e.Key is Key.Down ? grid.SelectedIndex + 1 : grid.SelectedIndex - 1;
                    if (index < 0)
                        index = 0;
                    if (index >= grid.Items.Count)
                        index = grid.Items.Count - 1;
                    grid.SelectedIndex = index;
                    var item = grid.Items[grid.SelectedIndex] as FirewallRuleViewModel;
                    if (item is not null)
                        grid.ScrollIntoView(item);
                    grid.UpdateLayout();
                    row = grid.ItemContainerGenerator.ContainerFromIndex(grid.SelectedIndex) as DataGridRow;
                    var viewModel = grid.DataContext as FirewallViewModel;
                    viewModel?.SelectedRule = item;
                }
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(grid), null);
                Keyboard.ClearFocus();
                // DataGridRow is not focusable.
                if (row is not null)
                {
                    var cell = GetCell(grid, row, 1);
                    cell?.Focus();
                }
                e.Handled = true;
                break;
            case Key.W when Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl):
                dataContext.OpenWindowsFirewallCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }
    
    /// <summary>
    /// Gets a DataGridCell from a given DataGrid, given row and column accounting for virtualization.
    /// </summary>
    /// <param name="grid">The DataGrid to retrieve the cell from.</param>
    /// <param name="row">The row instance to get the index from.</param>
    /// <param name="column">The column index.</param>
    /// <returns>A DataGridCell instance.</returns>
    private static DataGridCell GetCell(DataGrid grid, DataGridRow row, int column)
    {
        if (row == null) return null;

        var presenter = GetVisualChild<System.Windows.Controls.Primitives.DataGridCellsPresenter>(row);
        if (presenter != null)
            return presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
        grid.ScrollIntoView(row, grid.Columns[column]);
        presenter = GetVisualChild<System.Windows.Controls.Primitives.DataGridCellsPresenter>(row);

        return presenter?.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
    }
    
    /// <summary>
    /// Get the first child of a Visual, which is not null. 
    /// </summary>
    /// <param name="parent">Parent of children.</param>
    /// <typeparam name="T">Any Visual type.</typeparam>
    /// <returns>First non-null child found or null.</returns>
    private static T GetVisualChild<T>(Visual parent) where T : Visual
    {
        T child = null;
        int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < numVisuals; i++)
        {
            var v = (Visual)VisualTreeHelper.GetChild(parent, i);
            child = v as T ?? GetVisualChild<T>(v);
            if (child != null)
            {
                break;
            }
        }
        return child;
    }

    /// <summary>
    /// Restore the focus to the RuleGrid, whenever it is lost by column sorting or button clicks.
    /// Also called on loading.
    /// </summary>
    public void RestoreRuleGridFocus()
    {
        if (RuleGrid is null)
            return;

        // Post-focus: let WPF finish the click/command that caused focus to move away.
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (!IsVisible || !IsEnabled)
                return;

            if (!RuleGrid.HasItems)
            {
                RuleGrid.Focus();
                return;
            }

            if (RuleGrid.SelectedIndex < 0)
                RuleGrid.SelectedIndex = 0;

            RuleGrid.ScrollIntoView(RuleGrid.SelectedItem);
            RuleGrid.UpdateLayout();

            // Prefer focusing a real cell (more reliable than focusing the DataGrid itself).
            var row = RuleGrid.ItemContainerGenerator.ContainerFromIndex(RuleGrid.SelectedIndex) as DataGridRow;
            if (row != null)
            {
                // Pick a sensible column (0 = expander button column; 1 = "Name" in your grid).
                var cell = GetCell(RuleGrid, row, column: 1) ?? GetCell(RuleGrid, row, column: 0);
                if (cell != null)
                {
                    cell.Focus();
                    Keyboard.Focus(cell);
                    return;
                }
            }

            RuleGrid.Focus();
            Keyboard.Focus(RuleGrid);
        }), DispatcherPriority.Input);
    }
    
    /// <summary>
    /// Delegate the refocusing to <see cref="RestoreRuleGridFocus"/> on sorting,
    /// but with another dispatcher context.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RuleGrid_OnSorting(object sender, DataGridSortingEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(RestoreRuleGridFocus), DispatcherPriority.ContextIdle);
    }
    #endregion

}