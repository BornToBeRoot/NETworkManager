using System;
using System.Windows;
using NETworkManager.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Controls;
using System.Windows.Media;

namespace NETworkManager.Views;

public partial class IPScannerView
{
    private readonly IPScannerViewModel _viewModel;

    public IPScannerView(Guid tabId, string hostOrIPRange = null)
    {
        InitializeComponent();

        _viewModel = new IPScannerViewModel(DialogCoordinator.Instance, tabId, hostOrIPRange);

        DataContext = _viewModel;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnLoaded();
    }

    private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
    {
        _viewModel.OnClose();
    }

    public void CloseTab()
    {
        _viewModel.OnClose();
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is not ContextMenu menu) 
            return;
        
        // Set DataContext to ViewModel
        menu.DataContext = _viewModel;

        // Append custom commands
        var index = menu.Items.Count - 1;

        var entryFound = false;

        for (var i = 0; i < menu.Items.Count; i++)
        {
            if (menu.Items[i] is not MenuItem item) 
                continue;
                
            if ((string)item.Tag != "CustomCommands")
                continue;

            index = i;

            entryFound = true;

            break;
        }

        if (!entryFound)
            return;

        // Clear existing items in custom commands
        ((MenuItem)menu.Items[index])?.Items.Clear();

        // Add items to custom commands
        foreach (var info in IPScannerViewModel.CustomCommands)
        {
            ((MenuItem)menu.Items[index])?.Items.Add(new MenuItem
            {
                Header = info.Name,
                Command = _viewModel.CustomCommandCommand,
                CommandParameter = info.ID
            });
        }
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        // Get the row from the sender
        for (var visual = sender as Visual; visual != null; visual = VisualTreeHelper.GetParent(visual) as Visual)
        {
            if (visual is not DataGridRow row)
                continue;
            
            row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            
            break;
        }
    }
}
