using MahApps.Metro.Controls;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        #region Variables
        private NetworkConnectionView _networkConnectionView;
        #endregion

        #region Constructor
        public StatusWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = this;

            _networkConnectionView = new NetworkConnectionView();
            ContentControlNetworkConnection.Content = _networkConnectionView;
        
        }
        #endregion

        #region ICommands & Actions

        #endregion

        #region Methods
        public void ShowFromExternal()
        {
            ShowWindow();

            //Refresh();
        }

        private void ShowWindow()
        {
            // Show on primary screen in left/bottom corner
            // ToDo: User setting...
            Left = Screen.PrimaryScreen.WorkingArea.Right - Width - 10;
            Top = Screen.PrimaryScreen.WorkingArea.Bottom - Height - 10;

            Show();

            // ToDo: User setting...
            Topmost = true;
        }
        #endregion

        #region Events

        #endregion

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
        }
    }
}
