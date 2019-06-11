using System;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Utilities;
using NETworkManager.ViewModels;

namespace NETworkManager.Views
{
    public partial class IPScannerView
    {
        private readonly IPScannerViewModel _viewModel;

        public IPScannerView(int tabId, string hostOrIPRange = null)
        {
            InitializeComponent();

            _viewModel = new IPScannerViewModel(DialogCoordinator.Instance, tabId, hostOrIPRange);

            DataContext = _viewModel;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
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

        private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
            {
                // Set DataContext to ViewModel
                menu.DataContext = _viewModel;

                // Append custom commands
                int index = menu.Items.Count -1;
                                
                for (int i = 0; i < menu.Items.Count; i++ )
                {
                    if(menu.Items[i] is MenuItem item)
                    {
                        if (item.Name != "CustomCommands")
                            continue;

                        index = i;

                        break;
                    }
                }

                // Clear existing items in custom commands
                ((MenuItem)menu.Items[index]).Items.Clear();

                // Add items to custom commands
                foreach (CustomCommandInfo info in _viewModel.CustomCommands)
                {
                    ((MenuItem)menu.Items[index]).Items.Add(new MenuItem { Header = info.Name, Command = _viewModel.CustomCommandCommand, CommandParameter = info.ID });
                }
            }
        }                
    }
}
