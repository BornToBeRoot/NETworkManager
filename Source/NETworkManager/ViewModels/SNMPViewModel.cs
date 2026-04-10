using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
using NETworkManager.Localization;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Export;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace NETworkManager.ViewModels;

public class SNMPViewModel : ViewModelBase
{
    #region Contructor, load settings

    public SNMPViewModel(Guid tabId, SNMPSessionInfo sessionInfo)
    {
        _isLoading = true;

        ConfigurationManager.Current.SNMPTabCount++;

        _tabId = tabId;
        Host = sessionInfo?.Host;

        // Set collection view
        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_HostHistory);
        OidHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_OidHistory);

        // Result view
        ResultsView = CollectionViewSource.GetDefaultView(QueryResults);

        // Custom comparer to sort by OID
        ((ListCollectionView)ResultsView).CustomSort = Comparer<SNMPInfo>.Create((x, y) =>
            SNMPOIDHelper.CompareOIDs(x.OID, y.OID));

        // OID
        Oid = sessionInfo?.OID;

        // Modes
        Modes = [SNMPMode.Get, SNMPMode.Walk, SNMPMode.Set];
        Mode = Modes.FirstOrDefault(x => x == sessionInfo?.Mode);

        // Versions (v1, v2c, v3)
        Versions = Enum.GetValues<SNMPVersion>().Cast<SNMPVersion>().ToList();
        Version = Versions.FirstOrDefault(x => x == sessionInfo?.Version);

        // Community
        if (Version != SNMPVersion.V3)
            Community = sessionInfo?.Community;

        // Security
        Securities = [SNMPV3Security.NoAuthNoPriv, SNMPV3Security.AuthNoPriv, SNMPV3Security.AuthPriv];
        Security = Securities.FirstOrDefault(x => x == sessionInfo?.Security);

        // Username
        if (Version == SNMPVersion.V3)
            Username = sessionInfo?.Username;

        // Auth
        AuthenticationProviders = [.. Enum.GetValues<SNMPV3AuthenticationProvider>().Cast<SNMPV3AuthenticationProvider>()];
        AuthenticationProvider = AuthenticationProviders.FirstOrDefault(x => x == sessionInfo?.AuthenticationProvider);

        if (Version == SNMPVersion.V3 && Security != SNMPV3Security.NoAuthNoPriv)
            Auth = sessionInfo?.Auth;

        // Priv
        PrivacyProviders = [.. Enum.GetValues<SNMPV3PrivacyProvider>().Cast<SNMPV3PrivacyProvider>()];
        PrivacyProvider = PrivacyProviders.FirstOrDefault(x => x == sessionInfo?.PrivacyProvider);

        if (Version == SNMPVersion.V3 && Security == SNMPV3Security.AuthPriv)
            Priv = sessionInfo?.Priv;

        _isLoading = false;
    }

    #endregion

    #region Variables
    private static readonly ILog Log = LogManager.GetLogger(typeof(SNMPViewModel));

    private CancellationTokenSource _cancellationTokenSource;

    private readonly Guid _tabId;
    private readonly bool _isLoading;
    private bool _closed;

    public string Host
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

    public ICollectionView HostHistoryView { get; }

    public IEnumerable<SNMPMode> Modes { get; set; }

    public SNMPMode Mode
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.SNMP_Mode = value;

            field = value;
            OnPropertyChanged();

            // Re-validate OID if mode changed
            OnPropertyChanged(nameof(Oid));
        }
    }

    public IEnumerable<SNMPVersion> Versions { get; }

    public SNMPVersion Version
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.SNMP_Version = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public string Oid
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

    public ICollectionView OidHistoryView { get; }

    public IEnumerable<SNMPV3Security> Securities { get; }

    public SNMPV3Security Security
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.SNMP_Security = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsCommunityEmpty
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public SecureString Community
    {
        get;
        set
        {
            if (value == field)
                return;

            // Validate the community string
            IsCommunityEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            field = value;
            OnPropertyChanged();
        }
    }

    public string Username
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

    public IEnumerable<SNMPV3AuthenticationProvider> AuthenticationProviders { get; }

    public SNMPV3AuthenticationProvider AuthenticationProvider
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.SNMP_AuthenticationProvider = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsAuthEmpty
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public SecureString Auth
    {
        get;
        set
        {
            if (value == field)
                return;

            // Validate the auth string
            IsAuthEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            field = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<SNMPV3PrivacyProvider> PrivacyProviders { get; }

    public SNMPV3PrivacyProvider PrivacyProvider
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.SNMP_PrivacyProvider = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsPrivEmpty
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public SecureString Priv
    {
        get;
        set
        {
            if (value == field)
                return;

            // Validate the auth string
            IsPrivEmpty = value == null || string.IsNullOrEmpty(SecureStringHelper.ConvertToString(value));

            field = value;
            OnPropertyChanged();
        }
    }

    public string Data
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

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

    public bool CancelScan
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

    public ObservableCollection<SNMPInfo> QueryResults
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
        }
    } = [];

    public ICollectionView ResultsView { get; }

    public SNMPInfo SelectedResult
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

    public IList SelectedResults
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new ArrayList();


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

    #endregion

    #region ICommands & Actions

    public ICommand WorkCommand => new RelayCommand(_ => WorkAction(), Work_CanExecute);

    private bool Work_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    private void WorkAction()
    {
        Work();
    }

    public ICommand OpenOIDProfilesCommand => new RelayCommand(_ => OpenOIDProfilesAction());


    private void OpenOIDProfilesAction()
    {
        OpenOIDProfileSelection().ConfigureAwait(false);
    }

    public ICommand ExportCommand => new RelayCommand(_ => ExportAction());

    private void ExportAction()
    {
        Export().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    private void Work()
    {
        if (IsRunning)
            StopWork();
        else
            StartWork();
    }

    private async void StartWork()
    {
        IsStatusMessageDisplayed = false;
        IsRunning = true;

        QueryResults.Clear();

        DragablzTabItem.SetTabHeader(_tabId, Host);

        // Try to parse the string into an IP-Address
        if (!IPAddress.TryParse(Host, out var ipAddress))
        {
            var dnsResult =
                await DNSClientHelper.ResolveAorAaaaAsync(Host,
                    SettingsManager.Current.Network_ResolveHostnamePreferIPv4);

            if (dnsResult.HasError)
            {
                StatusMessage = DNSClientHelper.FormatDNSClientResultError(Host, dnsResult);
                IsStatusMessageDisplayed = true;
                IsRunning = false;
                return;
            }

            ipAddress = dnsResult.Value;
        }

        _cancellationTokenSource = new CancellationTokenSource();

        // SNMP...
        SNMPClient snmpClient = new();

        snmpClient.Received += SNMPClient_Received;
        snmpClient.DataUpdated += SNMPClient_DataUpdated;
        snmpClient.Error += SNMPClient_Error;
        snmpClient.Canceled += SNMPClient_Canceled;
        snmpClient.Complete += SNMPClient_Complete;

        var oidValue = Oid.Replace(" ", "");

        // Check if we have multiple OIDs for a Get request
        List<string> oids = new();

        if (Mode == SNMPMode.Get && oidValue.Contains(';'))
            oids.AddRange(oidValue.Split(';'));
        else
            oids.Add(oidValue);

        // SNMPv1 or v2c
        if (Version != SNMPVersion.V3)
        {
            var snmpOptions = new SNMPOptions(
                Community,
                Version,
                SettingsManager.Current.SNMP_Port,
                SettingsManager.Current.SNMP_WalkMode,
                _cancellationTokenSource.Token
            );

            switch (Mode)
            {
                case SNMPMode.Get:
                    snmpClient.GetAsync(ipAddress, oids, snmpOptions);
                    break;
                case SNMPMode.Walk:
                    snmpClient.WalkAsync(ipAddress, oids[0], snmpOptions);
                    break;
                case SNMPMode.Set:
                    snmpClient.SetAsync(ipAddress, oids[0], Data, snmpOptions);
                    break;
            }
        }
        // SNMPv3
        else
        {
            var snmpOptionsV3 = new SNMPOptionsV3(
                Security,
                Username,
                AuthenticationProvider,
                Auth,
                PrivacyProvider,
                Priv,
                SettingsManager.Current.SNMP_Port,
                SettingsManager.Current.SNMP_WalkMode,
                _cancellationTokenSource.Token
            );

            switch (Mode)
            {
                case SNMPMode.Get:
                    snmpClient.GetAsyncV3(ipAddress, oids, snmpOptionsV3);
                    break;
                case SNMPMode.Walk:
                    snmpClient.WalkAsyncV3(ipAddress, oids[0], snmpOptionsV3);
                    break;
                case SNMPMode.Set:
                    snmpClient.SetAsyncV3(ipAddress, oids[0], Data, snmpOptionsV3);
                    break;
            }
        }

        // Set timeout
        _cancellationTokenSource.CancelAfter(SettingsManager.Current.SNMP_Timeout);

        // Add to history...
        AddHostToHistory(Host);
        AddOidToHistory(Oid);
    }

    private void StopWork()
    {
        CancelScan = true;
        _cancellationTokenSource.Cancel();
    }

    public void OnClose()
    {
        // Prevent multiple calls
        if (_closed)
            return;

        _closed = true;

        ConfigurationManager.Current.SNMPTabCount--;
    }

    private async Task OpenOIDProfileSelection()
    {
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        var childWindow = new SNMPOIDProfilesChildWindow(window);

        var childWindowViewModel = new SNMPOIDProfilesViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            if (instance.SelectedOIDProfile == null)
                return;

            Mode = instance.SelectedOIDProfile.Mode;
            Oid = instance.SelectedOIDProfile?.OID;
        }, async _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        });

        childWindow.Title = Strings.SelectOIDProfile;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await window.ShowChildWindowAsync(childWindow);
    }

    private void AddHostToHistory(string host)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.SNMP_HostHistory.ToList(), host,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.SNMP_HostHistory.Clear();
        OnPropertyChanged(nameof(Host)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.SNMP_HostHistory.Add(x));
    }

    private void AddOidToHistory(string oid)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.SNMP_OidHistory.ToList(), oid,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.SNMP_OidHistory.Clear();
        OnPropertyChanged(nameof(Oid)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.SNMP_OidHistory.Add(x));
    }

    private Task Export()
    {
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        var childWindow = new ExportChildWindow();

        var childWindowViewModel = new ExportViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                ExportManager.Export(instance.FilePath, instance.FileType,
                    instance.ExportAll
                        ? QueryResults
                        : new ObservableCollection<SNMPInfo>(SelectedResults.Cast<SNMPInfo>().ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(window, Strings.Error,
                   Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                   Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.SNMP_ExportFileType = instance.FileType;
            SettingsManager.Current.SNMP_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.SNMP_ExportFileType, SettingsManager.Current.SNMP_ExportFilePath);

        childWindow.Title = Strings.Export;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return window.ShowChildWindowAsync(childWindow);
    }

    #endregion

    #region Events

    private void SNMPClient_Received(object sender, SNMPReceivedArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
            new Action(delegate { QueryResults.Add(e.Args); }));
    }

    private void SNMPClient_DataUpdated(object sender, EventArgs e)
    {
        StatusMessage = Strings.DataHasBeenUpdated;
        IsStatusMessageDisplayed = true;
    }

    private void SNMPClient_Error(object sender, SNMPErrorArgs e)
    {
        if (e.IsErrorCode)
            StatusMessage = ResourceTranslator.Translate(ResourceIdentifier.SNMPErrorCode, e.ErrorCode);
        else if (e.IsErrorCodeV3)
            StatusMessage = ResourceTranslator.Translate(ResourceIdentifier.SNMPV3ErrorCode, e.ErrorCodeV3);
        else
            StatusMessage = e.ErrorMessage;

        IsStatusMessageDisplayed = true;
    }

    private void SNMPClient_Canceled(object sender, EventArgs e)
    {
        StatusMessage = CancelScan
            ? Strings.CanceledByUserMessage
            : Strings.TimeoutOnSNMPQuery;
        IsStatusMessageDisplayed = true;
    }

    private void SNMPClient_Complete(object sender, EventArgs e)
    {
        CancelScan = false;
        IsRunning = false;
    }

    #endregion
}