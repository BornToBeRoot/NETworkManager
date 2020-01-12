using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System;
using System.Windows.Threading;
using NETworkManager.Utilities;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.WebConsole;

namespace NETworkManager.Controls
{
    public partial class WebConsoleControl : INotifyPropertyChanged
    {
        #region PropertyChangedEventHandler
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Variables
        private bool _initialized;
        private bool _closing;      // When the tab is closed --> OnClose()

        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly WebConsoleSessionInfo _sessionInfo;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (value == _isLoading)
                    return;

                _isLoading = value;
                OnPropertyChanged();
            }
        }

        private string _url;
        public string Url
        {
            get => _url;
            set
            {
                if (value == _url)
                    return;

                _url = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, load
        public WebConsoleControl(WebConsoleSessionInfo info)
        {
            InitializeComponent();
            DataContext = this;

            _dialogCoordinator = DialogCoordinator.Instance;

            _sessionInfo = info;

            Browser2.NavigationStarting += Browser2_NavigationStarting;
            Browser2.NavigationCompleted += Browser2_NavigationCompleted;

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }
              
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Connect after the control is drawn and only on the first init
            if (_initialized)
                return;

            Connect();

            _initialized = true;
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            CloseTab();
        }
        #endregion

        #region ICommands & Actions
        public ICommand RefreshCommand
        {
            get { return new RelayCommand(p => RefreshAction(), RefreshCommand_CanExecute); }
        }

        private bool RefreshCommand_CanExecute(object obj)
        {
            return !IsLoading;
        }

        private void RefreshAction()
        {
            Browser2.Refresh();
        }

        public ICommand GoBackCommand
        {
            get { return new RelayCommand(p => GoBackAction(), GoBackCommand_CanExecute); }
        }

        private bool GoBackCommand_CanExecute(object obj)
        {
            return !IsLoading && Browser2.CanGoBack;
        }

        private void GoBackAction()
        {
            Browser2.GoBack();
        }

        public ICommand GoForwardCommand
        {
            get { return new RelayCommand(p => GoForwardAction(), GoForwardCommand_CanExecute); }
        }

        private bool GoForwardCommand_CanExecute(object obj)
        {
            return !IsLoading && Browser2.CanGoForward;
        }

        private void GoForwardAction()
        {
            Browser2.GoForward();
        }
        #endregion

        #region Methods       
        private async void Connect()
        {
            //  while (!Browser.IsBrowserInitialized)
            //      await Task.Delay(250);

            //  Browser.Address = _sessionInfo.Url;

            Browser2.Navigate(_sessionInfo.Url);
        }

        public void CloseTab()
        {
            _closing = true;
        }
        #endregion

        #region Events
        private void Browser2_NavigationCompleted(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlNavigationCompletedEventArgs e)
        {
            Url = e.Uri.ToString();

            IsLoading = false;
        }

        private void Browser2_NavigationStarting(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlNavigationStartingEventArgs e)
        {            
            IsLoading = true;

            Url = e.Uri.ToString();
        }
        #endregion
    }
}
