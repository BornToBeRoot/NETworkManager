using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using log4net;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.IPApi;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the IP API DNS resolver widget.
/// </summary>
public class IPApiDNSResolverWidgetViewModel : ViewModelBase
{
    #region Variables

    /// <summary>
    /// A check was requested while a check was running (see <see cref="Check" />).
    /// </summary>
    private bool _recheckPending;

    private static readonly ILog Log = LogManager.GetLogger(typeof(IPApiDNSResolverWidgetViewModel));

    /// <summary>
    /// Gets or sets a value indicating whether the check is running.
    /// </summary>
    public bool IsRunning
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
    /// Gets the result of the DNS resolver check.
    /// </summary>
    public DNSResolverResult Result
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

    /// <summary>
    /// Gets the time of the last successful check, <c>null</c> before it and while checking (the values belong to it).
    /// </summary>
    public DateTime? LastCheckTime
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

    /// <summary>
    /// Message of the last check, e.g. an error or the rate limit (shown below the header).
    /// </summary>
    public string StatusMessage
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

    public StatusLevel StatusMessageLevel
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

    public bool IsStatusMessageDisplayed
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

    #endregion

    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="IPApiDNSResolverWidgetViewModel"/> class.
    /// </summary>
    public IPApiDNSResolverWidgetViewModel()
    {
        LoadSettings();
        Check();
    }

    /// <summary>
    /// Loads the settings.
    /// </summary>
    private void LoadSettings()
    {

    }

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to check the DNS resolver.
    /// </summary>
    public ICommand CheckCommand => new RelayCommand(_ => Check(), _ => !IsRunning);

    /// <summary>
    /// Gets the command to copy all values as text, e.g. for a support request.
    /// </summary>
    public ICommand CopyAllCommand => new RelayCommand(_ => CopyAllAction(), _ => !IsRunning && Result?.Info != null);

    /// <summary>
    /// Gets the command to open the IP address of the DNS server in another tool (parameter: <see cref="ApplicationName" />).
    /// </summary>
    public ICommand OpenDNSServerCommand => new RelayCommand(parameter => OpenApplication(parameter, Result?.Info?.DnsIp),
        _ => !string.IsNullOrEmpty(Result?.Info?.DnsIp));

    /// <summary>
    /// Gets the command to open the EDNS client subnet in another tool (parameter: <see cref="ApplicationName" />).
    /// </summary>
    public ICommand OpenEDNSCommand => new RelayCommand(parameter => OpenApplication(parameter, Result?.Info?.EdnsIp),
        _ => !string.IsNullOrEmpty(Result?.Info?.EdnsIp));

    /// <summary>
    /// Opens a value in another tool (context menu, parameter: <see cref="ApplicationName" />).
    /// </summary>
    private static void OpenApplication(object parameter, string target)
    {
        if (parameter is ApplicationName application && !string.IsNullOrEmpty(target))
            NavigationManager.Open(application, target);
    }

    /// <summary>
    /// Gets the command to enable the DNS resolver check (setting <c>Dashboard_CheckIPApiDNSResolver</c>, off by default: ip-api.com receives
    /// the IP address) and to check right away.
    /// </summary>
    public ICommand EnableCheckCommand => new RelayCommand(_ => EnableCheckAction());

    private void CopyAllAction()
    {
        var info = Result.Info;
        var text = new StringBuilder();

        foreach (var (title, ip, geo) in new[]
                 {
                     (Strings.DNS, info.DnsIp, info.DnsGeo),
                     (Strings.EDNS, info.EdnsIp, info.EdnsGeo)
                 })
        {
            if (text.Length > 0)
                text.AppendLine();

            text.AppendLine(title);
            text.AppendLine($"{Strings.IPAddress}: {FormatValue(ip)}");
            text.AppendLine($"{Strings.Geolocation}: {FormatValue(geo)}");
        }

        ClipboardHelper.SetClipboard(text.ToString().TrimEnd());
    }

    // Like the widget: "-/-" for an empty value (e.g. no EDNS client subnet)
    private static string FormatValue(string value) => string.IsNullOrEmpty(value) ? "-/-" : value;

    private void EnableCheckAction()
    {
        SettingsManager.Current.Dashboard_CheckIPApiDNSResolver = true;

        Check();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Checks the DNS resolver (also called by the dashboard, e.g. after a network change).
    /// </summary>
    public void Check()
    {
        // Already checking, e.g. the network changed right after the widget was created: the running request may use
        // the old network, check again once it has finished
        if (IsRunning)
        {
            _recheckPending = true;
            return;
        }

        _ = CheckAsync();
    }

    /// <summary>
    /// Checks the DNS resolver asynchronously.
    /// </summary>
    private async Task CheckAsync()
    {
        // Check is disabled via settings (the dashboard checks again when it is shown after the setting changed): no
        // values, message or time of an earlier check next to the "Enable" button
        if (!SettingsManager.Current.Dashboard_CheckIPApiDNSResolver)
        {
            IsStatusMessageDisplayed = false;
            Result = null;
            LastCheckTime = null;

            return;
        }

        IsRunning = true;
        IsStatusMessageDisplayed = false;
        Result = null;
        LastCheckTime = null;

        try
        {
            // Takes at least the refresh interval, so the reload animation is visible and the check can't be spammed -
            // the request runs at the same time
            var result = await TaskHelper.WithMinimumDurationAsync(
                () => ScreenshotData.IsEnabled
                    ? Task.FromResult(ScreenshotData.CreateDNSResolverResult())
                    : DNSResolverService.GetInstance().GetDNSResolverAsync(),
                GlobalStaticConfiguration.ApplicationUIRefreshInterval);

            if (result.HasError)
            {
                Log.Error($"edns.ip-api.com error: {result.ErrorMessage}, error code: {result.ErrorCode}");

                StatusMessage = WebRequestErrorMessage.Get(DNSResolverService.Host, DNSResolverService.RequestTimeout,
                    result.Error);
                StatusMessageLevel = StatusLevel.Error;
                IsStatusMessageDisplayed = true;
            }
            else
            {
                Result = result;
                LastCheckTime = DateTime.Now;
            }
        }
        finally
        {
            IsRunning = false;

            // Enable the refresh button (CanExecute is otherwise only evaluated on the next input)
            CommandManager.InvalidateRequerySuggested();
        }

        // A check requested in the meantime (e.g. a network change)
        if (_recheckPending)
        {
            _recheckPending = false;
            Check();
        }
    }

    #endregion
}