using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

public partial class ImportProfilesResultChildWindow
{
    public ImportProfilesResultChildWindow(Window parentWindow)
    {
        InitializeComponent();

        ChildWindowMaxWidth = 1050;
        ChildWindowMaxHeight = 650;
        ChildWindowWidth = parentWindow.ActualWidth * 0.85;
        //ChildWindowHeight = parentWindow.ActualHeight * 0.85;

        parentWindow.SizeChanged += (_, _) =>
        {
            ChildWindowWidth = parentWindow.ActualWidth * 0.85;
            //ChildWindowHeight = parentWindow.ActualHeight * 0.85;
        };
    }

    private void ChildWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate
        {
            TextBoxSearch.Focus();
        }));
    }

    private void DataGrid_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Space || sender is not DataGrid dataGrid)
            return;

        var items = dataGrid.SelectedItems
            .OfType<ImportCandidateItem>()
            .ToList();

        if (items.Count == 0)
            return;

        var current = dataGrid.CurrentItem as ImportCandidateItem;
        var newValue = current != null ? !current.IsSelected : !items[0].IsSelected;

        foreach (var item in items)
            item.IsSelected = newValue;

        e.Handled = true;
    }
}