using MahApps.Metro.SimpleChildWindow;
using Microsoft.Web.WebView2.Core;
using NETworkManager.Localization.Resources;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NETworkManager.ViewModels;

public class WebConsoleSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    private bool _showAddressBar;

    public bool ShowAddressBar
    {
        get => _showAddressBar;
        set
        {
            if (value == _showAddressBar)
                return;

            if (!_isLoading)
                SettingsManager.Current.WebConsole_ShowAddressBar = value;

            _showAddressBar = value;
            OnPropertyChanged();
        }
    }

    private bool _isStatusBarEnabled;

    public bool IsStatusBarEnabled
    {
        get => _isStatusBarEnabled;
        set
        {
            if (value == _isStatusBarEnabled)
                return;

            if (!_isLoading)
                SettingsManager.Current.WebConsole_IsStatusBarEnabled = value;

            _isStatusBarEnabled = value;
            OnPropertyChanged();
        }
    }

    private bool _isPasswordSaveEnabled;

    public bool IsPasswordSaveEnabled
    {
        get => _isPasswordSaveEnabled;
        set
        {
            if (value == _isPasswordSaveEnabled)
                return;

            if (!_isLoading)
                SettingsManager.Current.WebConsole_IsPasswordSaveEnabled = value;

            _isPasswordSaveEnabled = value;
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
        DeleteBrowsingData().ConfigureAwait(false);
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

        // Create a temporary WebView2 instance to clear browsing data
        var webView2Environment =
            await CoreWebView2Environment.CreateAsync(null, GlobalStaticConfiguration.WebConsole_Cache);

        var windowHwnd = new WindowInteropHelper(Application.Current.MainWindow).Handle;

        var webView2Controller = await webView2Environment.CreateCoreWebView2ControllerAsync(windowHwnd);

        await webView2Controller.CoreWebView2.Profile.ClearBrowsingDataAsync();

        webView2Controller.Close();
    }

    #endregion
}