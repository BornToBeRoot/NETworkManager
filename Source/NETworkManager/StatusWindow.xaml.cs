using MahApps.Metro.Controls;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace NETworkManager
{
    public partial class StatusWindow : MetroWindow, INotifyPropertyChanged
    {
        #region PropertyChangedEventHandler
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private MainWindow _mainWindow;

        private NetworkInterfaceInfo _networkInterfaceInfo;
        public NetworkInterfaceInfo NetworkInterfaceInfo
        {
            get => _networkInterfaceInfo;
            set
            {
                if (value == _networkInterfaceInfo)
                    return;

                _networkInterfaceInfo = value;
                OnPropertyChanged();
            }
        }

        public StatusWindow(MainWindow mainWindow)
        {
            InitializeComponent();

            DataContext = this;

            _mainWindow = mainWindow;
        }

        #region Events

        private void MetroWindow_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Hide();
        }

        private void MetroWindow_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UpdateView();
        }
        #endregion

        #region ICommands & Actions
        public ICommand OpenMainWindowCommand => new RelayCommand(p => OpenMainWindowAction());

        private void OpenMainWindowAction()
        {
            if (_mainWindow.ShowWindowCommand.CanExecute(null))
                _mainWindow.ShowWindowCommand.Execute(null);
        }

        #endregion

        #region Methods

        private void UpdateView()
        {
            RefreshNetwork();
        }

        private async void RefreshNetwork()
        {
            var detectedIP = await NetworkInterface.DetectLocalIPAddressBasedOnRoutingAsync(System.Net.IPAddress.Parse("1.1.1.1"));

            foreach (NetworkInterfaceInfo info in await NetworkInterface.GetNetworkInterfacesAsync())
            {
                if (info.IPv4Address.Contains(detectedIP))
                    NetworkInterfaceInfo = info;
            }
        }
        #endregion
    }
}
