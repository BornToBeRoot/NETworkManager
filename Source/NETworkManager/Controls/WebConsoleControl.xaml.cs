using System.ComponentModel;
using System.Windows;
using System;
using System.Windows.Threading;
using NETworkManager.Utilities;
using System.Windows.Input;
using NETworkManager.Models.WebConsole;
using Microsoft.Web.WebView2.Core;
using NETworkManager.Settings;

namespace NETworkManager.Controls;

public partial class WebConsoleControl : UserControlBase
{
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

    public bool ShowAddressBar => SettingsManager.Current.WebConsole_ShowAddressBar;
    #endregion

    #region Constructor, load
    public WebConsoleControl(WebConsoleSessionInfo info)
    {
        InitializeComponent();
        DataContext = this;

        _sessionInfo = info;

        Browser2.NavigationStarting += Browser2_NavigationStarting;
        Browser2.NavigationCompleted += Browser2_NavigationCompleted;        
        Browser2.SourceChanged += Browser2_SourceChanged;

        SettingsManager.Current.PropertyChanged += Current_PropertyChanged;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }
    
    private void Browser2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
    {
        Url = Browser2.Source.ToString();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Connect after the control is drawn and only on the first init
        if (_initialized)
            return;

        // Set user data folder - Fix #382            
        var webView2Environment = await CoreWebView2Environment.CreateAsync(null, GlobalStaticConfiguration.WebConsole_Cache);
        await Browser2.EnsureCoreWebView2Async(webView2Environment);

        Navigate(_sessionInfo.Url);

        _initialized = true;
    }
    #endregion
    
    #region ICommands & Actions

    private bool NavigateCommand_CanExecute(object obj) => !IsLoading;

    public ICommand NavigateCommand => new RelayCommand(p => NavigateAction(), NavigateCommand_CanExecute);

    private void NavigateAction()
    {
        Navigate(Url);        
    }

    private bool StopCommand_CanExecute(object obj) => IsLoading;

    public ICommand StopCommand => new RelayCommand(p => StopAction(), StopCommand_CanExecute);

    private void StopAction()
    {
        Stop();
    }

    private bool ReloadCommand_CanExecute(object obj) => !IsLoading;

    public ICommand ReloadCommand => new RelayCommand(p => ReloadAction(), ReloadCommand_CanExecute);
    
    private void ReloadAction()
    {
        Browser2.Reload();        
    }

    private bool GoBackCommand_CanExecute(object obj) => !IsLoading && Browser2.CanGoBack;

    public ICommand GoBackCommand => new RelayCommand(p => GoBackAction(), GoBackCommand_CanExecute);

    private void GoBackAction()
    {
        Browser2.GoBack();
    }

    private bool GoForwardCommand_CanExecute(object obj) => !IsLoading && Browser2.CanGoForward;

    public ICommand GoForwardCommand => new RelayCommand(p => GoForwardAction(), GoForwardCommand_CanExecute);

    private void GoForwardAction()
    {
        Browser2.GoForward();
    }
    #endregion

    #region Methods       
    private void Navigate(string url)
    {
        Browser2.Source = new Uri(url);
    }

    private void Stop()
    {
        Browser2.Stop();
    }

    public void CloseTab()
    {

    }
    #endregion

    #region Events
    private void Browser2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
    {
        IsLoading = true;
    }
    
    private void Browser2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (FirstLoad)
            FirstLoad = false;

        IsLoading = false;
    }

    private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SettingsInfo.WebConsole_ShowAddressBar):
                OnPropertyChanged(nameof(ShowAddressBar));
                break;
        }
    }

    private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
    {
        CloseTab();
    }
    #endregion
}
