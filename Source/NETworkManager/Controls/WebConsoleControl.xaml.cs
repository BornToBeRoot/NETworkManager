﻿using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using NETworkManager.Models.WebConsole;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.Controls;

public partial class WebConsoleControl : UserControlBase, IDragablzTabItem
{
    #region Variables

    private bool _initialized;
    private bool _closed;

    private readonly Guid _tabId;
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

    public WebConsoleControl(Guid tabId, WebConsoleSessionInfo sessionInfo)
    {
        InitializeComponent();
        DataContext = this;

        ConfigurationManager.Current.WebConsoleTabCount++;

        _tabId = tabId;
        _sessionInfo = sessionInfo;

        Browser.NavigationStarting += Browser2_NavigationStarting;
        Browser.NavigationCompleted += Browser2_NavigationCompleted;
        Browser.SourceChanged += Browser2_SourceChanged;

        Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
    }

    private void Browser2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
    {
        Url = Browser.Source.ToString();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Connect after the control is drawn and only on the first init
        if (_initialized)
            return;

        // Set user data folder - Fix #382            
        var webView2Environment =
            await CoreWebView2Environment.CreateAsync(null, GlobalStaticConfiguration.WebConsole_Cache);
        
        await Browser.EnsureCoreWebView2Async(webView2Environment);

        //await Browser.CoreWebView2.Profile.ClearBrowsingDataAsync();

        // Set the default settings
        Browser.CoreWebView2.Settings.IsStatusBarEnabled = true;
        Browser.CoreWebView2.Settings.AreDevToolsEnabled = false;
        Browser.CoreWebView2.Settings.IsGeneralAutofillEnabled = true;
        Browser.CoreWebView2.Settings.IsPasswordAutosaveEnabled = true;

        Navigate(_sessionInfo.Url);

        _initialized = true;
    }

    #endregion

    #region ICommands & Actions

    private bool NavigateCommand_CanExecute(object obj)
    {
        return !IsLoading;
    }

    public ICommand NavigateCommand => new RelayCommand(_ => NavigateAction(), NavigateCommand_CanExecute);

    private void NavigateAction()
    {
        Navigate(Url);
    }

    private bool StopCommand_CanExecute(object obj)
    {
        return IsLoading;
    }

    public ICommand StopCommand => new RelayCommand(_ => StopAction(), StopCommand_CanExecute);

    private void StopAction()
    {
        Stop();
    }

    private bool ReloadCommand_CanExecute(object obj)
    {
        return !IsLoading;
    }

    public ICommand ReloadCommand => new RelayCommand(_ => ReloadAction(), ReloadCommand_CanExecute);

    private void ReloadAction()
    {
        Browser.Reload();
    }

    private bool GoBackCommand_CanExecute(object obj)
    {
        return !IsLoading && Browser.CanGoBack;
    }

    public ICommand GoBackCommand => new RelayCommand(_ => GoBackAction(), GoBackCommand_CanExecute);

    private void GoBackAction()
    {
        Browser.GoBack();
    }

    private bool GoForwardCommand_CanExecute(object obj)
    {
        return !IsLoading && Browser.CanGoForward;
    }

    public ICommand GoForwardCommand => new RelayCommand(_ => GoForwardAction(), GoForwardCommand_CanExecute);

    private void GoForwardAction()
    {
        Browser.GoForward();
    }

    #endregion

    #region Methods

    private void Navigate(string url)
    {
        Browser.Source = new Uri(url);
    }

    private void Stop()
    {
        Browser.Stop();
        
        
    }

    public void CloseTab()
    {
        // Prevent multiple calls
        if (_closed)
            return;

        _closed = true;

        ConfigurationManager.Current.WebConsoleTabCount--;
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

    private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
    {
        CloseTab();
    }

    #endregion
}