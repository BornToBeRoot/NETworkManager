﻿using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class PortScannerSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    private readonly IDialogCoordinator _dialogCoordinator;

    public ICollectionView PortProfiles { get; }

    private PortProfileInfo _selectedPortProfile = new();

    public PortProfileInfo SelectedPortProfile
    {
        get => _selectedPortProfile;
        set
        {
            if (value == _selectedPortProfile)
                return;

            _selectedPortProfile = value;
            OnPropertyChanged();
        }
    }

    private bool _showAllResults;

    public bool ShowAllResults
    {
        get => _showAllResults;
        set
        {
            if (value == _showAllResults)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_ShowAllResults = value;

            _showAllResults = value;
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
                SettingsManager.Current.PortScanner_Timeout = value;

            _timeout = value;
            OnPropertyChanged();
        }
    }

    private bool _resolveHostname;

    public bool ResolveHostname
    {
        get => _resolveHostname;
        set
        {
            if (value == _resolveHostname)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_ResolveHostname = value;

            _resolveHostname = value;
            OnPropertyChanged();
        }
    }

    private int _maxHostThreads;

    public int MaxHostThreads
    {
        get => _maxHostThreads;
        set
        {
            if (value == _maxHostThreads)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_MaxHostThreads = value;

            _maxHostThreads = value;
            OnPropertyChanged();
        }
    }

    private int _maxPortThreads;

    public int MaxPortThreads
    {
        get => _maxPortThreads;
        set
        {
            if (value == _maxPortThreads)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_MaxPortThreads = value;

            _maxPortThreads = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    public PortScannerSettingsViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        PortProfiles = CollectionViewSource.GetDefaultView(SettingsManager.Current.PortScanner_PortProfiles);
        PortProfiles.SortDescriptions.Add(
            new SortDescription(nameof(PortProfileInfo.Name), ListSortDirection.Ascending));

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ShowAllResults = SettingsManager.Current.PortScanner_ShowAllResults;
        Timeout = SettingsManager.Current.PortScanner_Timeout;
        ResolveHostname = SettingsManager.Current.PortScanner_ResolveHostname;
        MaxHostThreads = SettingsManager.Current.PortScanner_MaxHostThreads;
        MaxPortThreads = SettingsManager.Current.PortScanner_MaxPortThreads;
    }

    #endregion

    #region ICommand & Actions

    public ICommand AddPortProfileCommand => new RelayCommand(_ => AddPortProfileAction());

    private void AddPortProfileAction()
    {
        AddPortProfile().ConfigureAwait(false);
    }

    public ICommand EditPortProfileCommand => new RelayCommand(_ => EditPortProfileAction());

    private void EditPortProfileAction()
    {
        EditPortProfile().ConfigureAwait(false);
    }

    public ICommand DeletePortProfileCommand => new RelayCommand(_ => DeletePortProfileAction());

    private void DeletePortProfileAction()
    {
        DeletePortProfile().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    private async Task AddPortProfile()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.AddPortProfile
        };

        var viewModel = new PortProfileViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            SettingsManager.Current.PortScanner_PortProfiles.Add(new PortProfileInfo(instance.Name, instance.Ports));
        }, async _ => { await _dialogCoordinator.HideMetroDialogAsync(this, customDialog); });

        customDialog.Content = new PortProfileDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    public async Task EditPortProfile()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.EditPortProfile
        };

        var viewModel = new PortProfileViewModel(async instance =>
            {
                await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.PortScanner_PortProfiles.Remove(SelectedPortProfile);
                SettingsManager.Current.PortScanner_PortProfiles.Add(new PortProfileInfo(instance.Name,
                    instance.Ports));
            }, async _ => { await _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, true,
            SelectedPortProfile);

        customDialog.Content = new PortProfileDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    private Task DeletePortProfile()
    {
        var childWindow = new OKCancelInfoMessageChildWindow();

        var childWindowViewModel = new OKCancelInfoMessageViewModel(_ =>
            {
                childWindow.IsOpen = false;
                ConfigurationManager.Current.IsChildWindowOpen = false;

                SettingsManager.Current.PortScanner_PortProfiles.Remove(SelectedPortProfile);
            }, _ =>
            {
                childWindow.IsOpen = false;
                ConfigurationManager.Current.IsChildWindowOpen = false;
            },
            Strings.DeletePortProfileMessage, Strings.Delete);

        childWindow.Title = Strings.DeletePortProfile;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return (Application.Current.MainWindow as MainWindow).ShowChildWindowAsync(childWindow);
    }

    #endregion
}