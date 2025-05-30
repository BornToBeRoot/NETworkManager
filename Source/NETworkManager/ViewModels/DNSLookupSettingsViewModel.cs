﻿using DnsClient;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class DNSLookupSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;
    private readonly ServerConnectionInfo _profileDialogDefaultValues = new("10.0.0.1", 53, TransportProtocol.Udp);

    private readonly IDialogCoordinator _dialogCoordinator;

    public ICollectionView DNSServers { get; }

    private DNSServerConnectionInfoProfile _selectedDNSServer = new();

    public DNSServerConnectionInfoProfile SelectedDNSServer
    {
        get => _selectedDNSServer;
        set
        {
            if (value == _selectedDNSServer)
                return;

            _selectedDNSServer = value;
            OnPropertyChanged();
        }
    }

    private List<string> ServerInfoProfileNames => [.. SettingsManager.Current.DNSLookup_DNSServers
        .Where(x => !x.UseWindowsDNSServer).Select(x => x.Name)];

    private bool _addDNSSuffix;

    public bool AddDNSSuffix
    {
        get => _addDNSSuffix;
        set
        {
            if (value == _addDNSSuffix)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_AddDNSSuffix = value;

            _addDNSSuffix = value;
            OnPropertyChanged();
        }
    }

    private bool _useCustomDNSSuffix;

    public bool UseCustomDNSSuffix
    {
        get => _useCustomDNSSuffix;
        set
        {
            if (value == _useCustomDNSSuffix)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_UseCustomDNSSuffix = value;

            _useCustomDNSSuffix = value;
            OnPropertyChanged();
        }
    }

    private string _customDNSSuffix;

    public string CustomDNSSuffix
    {
        get => _customDNSSuffix;
        set
        {
            if (value == _customDNSSuffix)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_CustomDNSSuffix = value;

            _customDNSSuffix = value;
            OnPropertyChanged();
        }
    }

    private bool _recursion;

    public bool Recursion
    {
        get => _recursion;
        set
        {
            if (value == _recursion)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_Recursion = value;

            _recursion = value;
            OnPropertyChanged();
        }
    }

    private bool _useCache;

    public bool UseCache
    {
        get => _useCache;
        set
        {
            if (value == _useCache)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_UseCache = value;

            _useCache = value;
            OnPropertyChanged();
        }
    }

    public List<QueryClass> QueryClasses { get; private set; }

    private QueryClass _queryClass;

    public QueryClass QueryClass
    {
        get => _queryClass;
        set
        {
            if (value == _queryClass)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_QueryClass = value;

            _queryClass = value;
            OnPropertyChanged();
        }
    }

    /* 
     * Disabled until more query types are implemented.
     * 
    
    private bool _showOnlyMostCommonQueryTypes;

    public bool ShowOnlyMostCommonQueryTypes
    {
        get => _showOnlyMostCommonQueryTypes;
        set
        {
            if (value == _showOnlyMostCommonQueryTypes)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_ShowOnlyMostCommonQueryTypes = value;

            _showOnlyMostCommonQueryTypes = value;
            OnPropertyChanged();
        }
    }
    */

    private bool _useTCPOnly;

    public bool UseTCPOnly
    {
        get => _useTCPOnly;
        set
        {
            if (value == _useTCPOnly)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_UseTCPOnly = value;

            _useTCPOnly = value;
            OnPropertyChanged();
        }
    }

    private int _retries;

    public int Retries
    {
        get => _retries;
        set
        {
            if (value == _retries)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_Retries = value;

            _retries = value;
            OnPropertyChanged();
        }
    }

    private int _timeout;

    public int Timeout
    {
        get => _timeout;
        set
        {
            if (value == _timeout)
                return;

            if (!_isLoading)
                SettingsManager.Current.DNSLookup_Timeout = value;

            _timeout = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    public DNSLookupSettingsViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        DNSServers = CollectionViewSource.GetDefaultView(SettingsManager.Current.DNSLookup_DNSServers);
        DNSServers.SortDescriptions.Add(new SortDescription(nameof(DNSServerConnectionInfoProfile.Name),
            ListSortDirection.Ascending));
        DNSServers.Filter = o =>
        {
            if (o is not DNSServerConnectionInfoProfile info)
                return false;

            return !info.UseWindowsDNSServer;
        };

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        AddDNSSuffix = SettingsManager.Current.DNSLookup_AddDNSSuffix;
        UseCustomDNSSuffix = SettingsManager.Current.DNSLookup_UseCustomDNSSuffix;
        CustomDNSSuffix = SettingsManager.Current.DNSLookup_CustomDNSSuffix;
        Recursion = SettingsManager.Current.DNSLookup_Recursion;
        UseCache = SettingsManager.Current.DNSLookup_UseCache;
        QueryClasses = Enum.GetValues(typeof(QueryClass)).Cast<QueryClass>().OrderBy(x => x.ToString()).ToList();
        QueryClass = QueryClasses.First(x => x == SettingsManager.Current.DNSLookup_QueryClass);
        //ShowOnlyMostCommonQueryTypes = SettingsManager.Current.DNSLookup_ShowOnlyMostCommonQueryTypes;
        UseTCPOnly = SettingsManager.Current.DNSLookup_UseTCPOnly;
        Retries = SettingsManager.Current.DNSLookup_Retries;
        Timeout = SettingsManager.Current.DNSLookup_Timeout;
    }

    #endregion

    #region ICommand & Actions

    public ICommand AddDNSServerCommand => new RelayCommand(_ => AddDNSServerAction());

    private void AddDNSServerAction()
    {
        AddDNSServer().ConfigureAwait(false);
    }

    public ICommand EditDNSServerCommand => new RelayCommand(_ => EditDNSServerAction());

    private void EditDNSServerAction()
    {
        EditDNSServer().ConfigureAwait(false);
    }

    public ICommand DeleteDNSServerCommand => new RelayCommand(_ => DeleteDNSServerAction());

    private void DeleteDNSServerAction()
    {
        DeleteDNSServer().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    private async Task AddDNSServer()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.AddDNSServer
        };

        var viewModel = new ServerConnectionInfoProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.DNSLookup_DNSServers.Add(
                    new DNSServerConnectionInfoProfile(instance.Name, instance.Servers.ToList()));
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
            (ServerInfoProfileNames, false, true),
            _profileDialogDefaultValues);

        customDialog.Content = new ServerConnectionInfoProfileDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    public async Task EditDNSServer()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.EditDNSServer
        };

        var viewModel = new ServerConnectionInfoProfileViewModel(instance =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.DNSLookup_DNSServers.Remove(SelectedDNSServer);
                SettingsManager.Current.DNSLookup_DNSServers.Add(
                    new DNSServerConnectionInfoProfile(instance.Name, instance.Servers.ToList()));
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
            (ServerInfoProfileNames, true, true),
            _profileDialogDefaultValues, SelectedDNSServer);

        customDialog.Content = new ServerConnectionInfoProfileDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    private async Task DeleteDNSServer()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.DeleteDNSServer
        };

        var viewModel = new ConfirmDeleteViewModel(_ =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.DNSLookup_DNSServers.Remove(SelectedDNSServer);
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
            Strings.DeleteDNSServerMessage);

        customDialog.Content = new ConfirmDeleteDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    #endregion
}