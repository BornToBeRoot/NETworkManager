using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System;
using System.Windows.Threading;
using NETworkManager.Utilities;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Models.WebConsole;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using CefSharp.Wpf;
using CefSharp.Handler;
using CefSharp;

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
        #endregion

        #region Constructor, load
        public WebConsoleControl(WebConsoleSessionInfo info)
        {
            InitializeComponent();
            DataContext = this;

            _dialogCoordinator = DialogCoordinator.Instance;

            _sessionInfo = info;

            Browser.RequestHandler = new SslRequestHandler();

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
        public ICommand ReloadCommand
        {
            get { return new RelayCommand(p => ReloadAction()); }
        }

        private void ReloadAction()
        {
            Reload();
        }
        #endregion

        #region Methods       
        private async void Connect()
        {
            while (!Browser.IsBrowserInitialized)
                await Task.Delay(50);

            Browser.Address = _sessionInfo.Url;
        }

        private void Reload()
        {

        }

        public void CloseTab()
        {
            _closing = true;

        }
        #endregion

        #region Events

        #endregion
    }
}

public class SslRequestHandler : RequestHandler
{
    protected override bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
    {        
        Task.Run(() =>
        {
            //NOTE: When executing the callback in an async fashion need to check to see if it's disposed
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    callback.Continue(true);
                    
                    /*
                    //We'll allow the expired certificate from badssl.com
                    if (requestUrl.ToLower().Contains("https://expired.badssl.com/"))
                    {
                        callback.Continue(true);
                    }
                    else
                    {
                        callback.Continue(false);
                    }
                    */
                }
            }
        });        

        return true;
    }
}