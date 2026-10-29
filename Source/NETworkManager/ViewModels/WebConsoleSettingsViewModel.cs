using log4net;
using MahApps.Metro.SimpleChildWindow;
using Microsoft.Web.WebView2.Core;
using NETworkManager.Localization.Resources;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NETworkManager.ViewModels;

public class WebConsoleSettingsViewModel : ViewModelBase
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(WebConsoleSettingsViewModel));

    private readonly bool _isLoading;

    /// <summary>
    /// Gets or sets a value indicating whether the result of deleting the browsing data is displayed.
    /// </summary>
    public bool IsDeleteBrowsingDataStatusMessageDisplayed
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the result of deleting the browsing data.
    /// </summary>
    public string DeleteBrowsingDataStatusMessage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool ShowAddressBar
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.WebConsole_ShowAddressBar = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsStatusBarEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.WebConsole_IsStatusBarEnabled = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsPasswordSaveEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.WebConsole_IsPasswordSaveEnabled = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    public WebConsoleSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ShowAddressBar = SettingsManager.Current.WebConsole_ShowAddressBar;
        IsStatusBarEnabled = SettingsManager.Current.WebConsole_IsStatusBarEnabled;
        IsPasswordSaveEnabled = SettingsManager.Current.WebConsole_IsPasswordSaveEnabled;
    }

    #endregion

    #region ICommands & Actions

    public ICommand DeleteBrowsingDataCommand => new RelayCommand(_ => DeleteBrowsingDataAction());

    private void DeleteBrowsingDataAction()
    {
        _ = DeleteBrowsingData();
    }

    #endregion

    #region Methods

    private async Task DeleteBrowsingData()
    {
        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            Strings.DeleteBrowsingData,
            Strings.DeleteBrowsingDataMessage,
            ChildWindowIcon.Info,
            Strings.Delete);

        if (!result)
            return;

        IsDeleteBrowsingDataStatusMessageDisplayed = false;

        CoreWebView2Controller webView2Controller = null;

        try
        {
            // Create a temporary WebView2 instance to clear browsing data
            var webView2Environment =
                await CoreWebView2Environment.CreateAsync(null, GlobalStaticConfiguration.WebConsole_Cache);

            var windowHwnd = new WindowInteropHelper(Application.Current.MainWindow).Handle;

            webView2Controller = await webView2Environment.CreateCoreWebView2ControllerAsync(windowHwnd);

            await webView2Controller.CoreWebView2.Profile.ClearBrowsingDataAsync();

            DeleteBrowsingDataStatusMessage = Strings.BrowsingDataSuccessfullyDeletedMessage;
        }
        catch (Exception ex)
        {
            // E.g. the WebView2 runtime is not installed
            Log.Error("Error while deleting the browsing data of the WebView2 profile.", ex);

            DeleteBrowsingDataStatusMessage = Strings.BrowsingDataErrorMessage;
        }
        finally
        {
            // Also on an error, otherwise the cache folder stays locked until the application is closed
            webView2Controller?.Close();
        }

        IsDeleteBrowsingDataStatusMessageDisplayed = true;
    }

    #endregion
}