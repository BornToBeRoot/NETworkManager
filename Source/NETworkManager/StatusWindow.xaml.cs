using MahApps.Metro.Controls;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        private MainWindow _mainWindow;
        private NetworkConnectionView _networkConnectionView;
        #endregion

        #region Constructor
        public StatusWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = this;

            _mainWindow = mainWindow;

            _networkConnectionView = new NetworkConnectionView();
            ContentControlNetworkConnection.Content = _networkConnectionView;
        }
        #endregion

        #region ICommands & Actions
        public ICommand ReloadCommand => new RelayCommand(p => ReloadAction());

        private void ReloadAction()
        {
            Reload();
        }

        public ICommand ShowMainWindowCommand => new RelayCommand(p => ShowMainWindowAction());

        private void ShowMainWindowAction()
        {
            Hide();

            if (_mainWindow.ShowWindowCommand.CanExecute(null))
                _mainWindow.ShowWindowCommand.Execute(null);
        }

        public ICommand CloseCommand => new RelayCommand(p => CloseAction());

        private void CloseAction()
        {
            Hide();
        }

        #endregion

        #region Methods
        private void Reload()
        {
            _networkConnectionView.Reload();
        }

        /// <summary>
        /// Show the window on the screen.
        /// </summary>
        /// <param name="activate">Focus the window (will automatically hide if the focus is lost).</param>
        public void ShowWindow(bool activate)
        {
            // Show on primary screen in left/bottom corner
            // ToDo: User setting...
            Left = Screen.PrimaryScreen.WorkingArea.Right - Width - 10;
            Top = Screen.PrimaryScreen.WorkingArea.Bottom - Height - 10;

            Show();

            if (activate)
                Activate();

            Topmost = true;
        }
        #endregion

        #region Events
        private void MetroWindow_Deactivated(object sender, System.EventArgs e)
        {
            Hide();
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            Hide();
        }

        #endregion


    }
}
