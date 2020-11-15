using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System;
using System.Windows.Threading;
using NETworkManager.Utilities;
using System.Windows.Input;
using NETworkManager.Models.WebConsole;
using Microsoft.Web.WebView2.Core;

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

        private bool _firstLoad = true;
        public bool FirstLoad
        {
            get => _firstLoad;
            set
            {
                if (value == _firstLoad)
                    return;

                _firstLoad = value;
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

            _sessionInfo = info;

            Browser2.NavigationStarting += Browser2_NavigationStarting;
            Browser2.NavigationCompleted += Browser2_NavigationCompleted;
            Browser2.WebMessageReceived += Browser2_WebMessageReceived;

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
        public ICommand NavigateCommand
        {
            get { return new RelayCommand(p => NavigateAction(), NavigateCommand_CanExecute); }
        }

        private bool NavigateCommand_CanExecute(object obj)
        {
            return !IsLoading;
        }

        private void NavigateAction()
        {
            Browser2.CoreWebView2.Navigate(Url);
        }

        public ICommand ReloadCommand
        {
            get { return new RelayCommand(p => ReloadAction(), ReloadCommand_CanExecute); }
        }

        private bool ReloadCommand_CanExecute(object obj)
        {
            return !IsLoading;
        }

        private void ReloadAction()
        {
            Browser2.Reload();
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
        private void Connect()
        {
            Browser2.Source = new Uri(_sessionInfo.Url);
        }

        public void CloseTab()
        {

        }
        #endregion

        #region Events
        private void Browser2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (FirstLoad)
                FirstLoad = false;

            IsLoading = false;
        }

        private void Browser2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            IsLoading = true;

            Url = e.Uri.ToString();
        }

        private void Browser2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string uri = e.TryGetWebMessageAsString();
            Url = uri;
        }
        #endregion
    }
}
