using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Export;
using NETworkManager.Models.RDAP;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Validators;
using NETworkManager.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class RDAPViewModel : ViewModelBase
{
    #region Variables

    private static readonly ILog Log = LogManager.GetLogger(typeof(RDAPViewModel));

    private readonly Guid _tabId;
    private readonly bool _isLoading;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _firstLoad = true;
    private bool _closed;

    public string Query
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

    public ICollectionView QueryHistoryView { get; }

    public List<RDAPQueryType> QueryTypes { get; } = [.. Enum.GetValues<RDAPQueryType>()];

    public RDAPQueryType QueryType
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.RDAP_QueryType = value;

            field = value;
            OnPropertyChanged();

            // Validate the query again for the new type.
            OnPropertyChanged(nameof(Query));
        }
    }

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

    public bool IsCanceling
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

    public bool IsResultVisible
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

    public List<RDAPObjectViewModel> Results
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
    ///     Show the raw JSON response(s) instead of the parsed view.
    /// </summary>
    public bool ShowRawJSON
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

    public string RawJSON
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
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

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

    /// <summary>
    ///     Offer to open the domain in the Whois tool, if no RDAP server is known for it.
    /// </summary>
    public bool IsOpenInWhoisVisible
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

    private RDAPResult _result;
    private string _whoisDomain;

    #endregion

    #region Contructor, load settings

    public RDAPViewModel(Guid tabId, string query, RDAPQueryType? queryType)
    {
        _isLoading = true;

        ConfigurationManager.Current.RDAPTabCount++;

        _tabId = tabId;
        Query = query;

        // Set collection view
        QueryHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.RDAP_QueryHistory);

        LoadSettings();

        // A query from a profile brings its own type.
        if (queryType != null)
            QueryType = queryType.Value;

        _isLoading = false;
    }

    public void OnLoaded()
    {
        if (!_firstLoad)
            return;

        if (!string.IsNullOrEmpty(Query))
            _ = RunQuery();

        _firstLoad = false;
    }

    private void LoadSettings()
    {
        QueryType = SettingsManager.Current.RDAP_QueryType;
    }

    #endregion

    #region ICommands & Actions

    public ICommand QueryCommand => new RelayCommand(_ => QueryAction(), Query_CanExecute);

    private bool Query_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    private void QueryAction()
    {
        if (IsRunning)
            Stop();
        else
            _ = RunQuery();
    }

    public ICommand OpenInWhoisCommand => new RelayCommand(_ => OpenInWhoisAction());

    private void OpenInWhoisAction()
    {
        NavigationManager.Open(new ApplicationRequest(ApplicationName.Whois) { Target = _whoisDomain });
    }

    /// <summary>
    ///     Opens a link of a notice (e.g. terms of service). Only URLs validated by <see cref="RDAPObjectViewModel" />
    ///     are passed to this command.
    /// </summary>
    public ICommand OpenUrlCommand => new RelayCommand(OpenUrlAction);

    private static void OpenUrlAction(object url)
    {
        ExternalProcessStarter.OpenUrl((string)url);
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    private void ExportAction()
    {
        _ = Export();
    }

    #endregion

    #region Methods

    private async Task RunQuery()
    {
        IsStatusMessageDisplayed = false;
        IsOpenInWhoisVisible = false;
        IsResultVisible = false;
        IsRunning = true;

        _result = null;
        Results = null;
        RawJSON = null;

        DragablzTabItem.SetTabHeader(_tabId, Query);

        var parseError = RDAPQueryParser.TryParse(QueryType, Query, out var input);

        if (parseError != RDAPQueryParseError.None)
        {
            ShowStatusMessage(RDAPQueryValidator.GetErrorMessage(parseError));
            IsRunning = false;

            return;
        }

        // A subdomain (e.g. a hostname) is queried as its registered domain, which is also used for the error messages
        // and Whois.
        var query = RDAPQueryParser.GetRegisteredDomain(input) ?? input;

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            _result = await RDAPClient.GetInstance().QueryAsync(query, GlobalStaticConfiguration.RDAP_CachePath,
                TimeSpan.FromMilliseconds(SettingsManager.Current.RDAP_Timeout),
                SettingsManager.Current.RDAP_FollowReferral, _cancellationTokenSource.Token);

            List<RDAPObjectViewModel> results =
            [
                // The "Result" header is already shown. Titles are only needed to tell registry and registrar apart.
                new(_result, _result.Referral == null ? null : Strings.Registry)
            ];

            if (_result.Referral != null)
                results.Add(new RDAPObjectViewModel(_result.Referral, Strings.Registrar));

            Results = results;
            RawJSON = string.Join(Environment.NewLine + Environment.NewLine,
                results.Select(x => $"// {x.RequestUrl}{Environment.NewLine}{x.FormattedJson}"));

            IsResultVisible = true;

            List<string> messages = [];

            if (query != input)
                messages.Add(string.Format(Strings.RDAPXIsNotARegisteredDomainShowingY, input.Value, query.Value));

            if (_result.ReferralError != null)
                messages.Add(GetReferralErrorMessage(_result.ReferralError, query));

            if (messages.Count > 0)
                ShowStatusMessage(string.Join(Environment.NewLine, messages));

            AddQueryToHistory(Query);
        }
        catch (OperationCanceledException)
        {
            ShowStatusMessage(Strings.CanceledByUserMessage);
        }
        catch (RDAPException ex)
        {
            ShowStatusMessage(GetErrorMessage(ex, query));

            if (ex.Kind == RDAPErrorKind.NoServer && query.Type is RDAPQueryType.Domain or RDAPQueryType.TLD)
            {
                _whoisDomain = query.DomainName;
                IsOpenInWhoisVisible = true;
            }
        }
        catch (Exception ex)
        {
            Log.Error("Error while querying RDAP.", ex);
            ShowStatusMessage(ex.Message);
        }
        finally
        {
            IsRunning = false;
            IsCanceling = false;
        }
    }

    private void Stop()
    {
        IsCanceling = true;
        _cancellationTokenSource?.Cancel();
    }

    private void ShowStatusMessage(string message)
    {
        StatusMessage = message;
        IsStatusMessageDisplayed = true;
    }

    private static string GetReferralErrorMessage(RDAPException ex, RDAPQuery query)
    {
        return ex.Kind switch
        {
            RDAPErrorKind.InsecureReferral => string.Format(Strings.RDAPReferralInsecureX, ex.RequestUrl),
            RDAPErrorKind.PrivateAddress => string.Format(Strings.RDAPReferralPrivateAddressX, ex.RequestUrl),
            _ => string.Format(Strings.RDAPReferralFailedX, GetErrorMessage(ex, query))
        };
    }

    private static string GetErrorMessage(RDAPException ex, RDAPQuery query)
    {
        // Prefer the description sent by the server.
        var serverMessage = string.Join(" - ",
            new[] { ex.ErrorTitle, ex.ErrorDescription }.Where(x => !string.IsNullOrWhiteSpace(x)));

        if (string.IsNullOrEmpty(serverMessage))
            serverMessage = ex.Message;

        return ex.Kind switch
        {
            RDAPErrorKind.NoServer => string.Format(
                query.Type == RDAPQueryType.Entity ? Strings.RDAPNoServerForEntityX : Strings.RDAPNoServerForX,
                query.Value),
            RDAPErrorKind.BootstrapUnavailable => string.Format(Strings.RDAPBootstrapUnavailableX, ex.Message),
            RDAPErrorKind.NotFound => string.Format(Strings.RDAPNotFoundX, query.Value),
            RDAPErrorKind.BadRequest => string.Format(Strings.RDAPBadRequestX, serverMessage),
            RDAPErrorKind.RateLimited => ex.RetryAfter != null
                ? string.Format(Strings.RDAPRateLimitedTryAgainInXSeconds, Math.Ceiling(ex.RetryAfter.Value.TotalSeconds))
                : Strings.RDAPRateLimited,
            RDAPErrorKind.NotImplemented => Strings.RDAPNotImplemented,
            RDAPErrorKind.InsecureRedirect => string.Format(Strings.RDAPInsecureRedirectX, ex.Message),
            RDAPErrorKind.TooManyRedirects => Strings.RDAPTooManyRedirects,
            RDAPErrorKind.InvalidResponse => string.Format(Strings.RDAPInvalidResponseX, ex.Message),
            RDAPErrorKind.Network => string.Format(Strings.RDAPNetworkErrorX, ex.Message),
            // The timeout of the query (from the settings) is used for each request.
            RDAPErrorKind.Timeout => string.Format(Strings.RDAPTimeoutAfterXSeconds,
                SettingsManager.Current.RDAP_Timeout / 1000.0),
            _ => string.Format(Strings.RDAPHttpErrorX, serverMessage)
        };
    }

    public void OnClose()
    {
        // Prevent multiple calls
        if (_closed)
            return;

        _closed = true;

        // Stop the query
        if (IsRunning)
            Stop();

        ConfigurationManager.Current.RDAPTabCount--;
    }

    private void AddQueryToHistory(string query)
    {
        ListHelper.Modify(SettingsManager.Current.RDAP_QueryHistory, query,
            SettingsManager.Current.General_HistoryListEntries);
    }

    /// <summary>
    ///     Gets the content for the export. JSON contains the responses exactly as sent by the server(s), as an array
    ///     if the registrar referral was followed.
    /// </summary>
    private string GetExportContent(ExportFileType fileType)
    {
        if (fileType == ExportFileType.Json)
            return _result.Referral == null
                ? _result.RawJson
                : $"[{Environment.NewLine}{_result.RawJson},{Environment.NewLine}{_result.Referral.RawJson}{Environment.NewLine}]";

        return string.Join(Environment.NewLine, Results.Select(x => x.ToText()));
    }

    private Task Export()
    {
        if (_result == null)
            return Task.CompletedTask;

        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        var childWindow = new ExportChildWindow();

        var childWindowViewModel = new ExportViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                ExportManager.Export(instance.FilePath, GetExportContent(instance.FileType));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(window, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                   Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.RDAP_ExportFileType = instance.FileType;
            SettingsManager.Current.RDAP_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Json, ExportFileType.Txt
        ], false, SettingsManager.Current.RDAP_ExportFileType, SettingsManager.Current.RDAP_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return window.ShowChildWindowAsync(childWindow);
    }

    #endregion
}
