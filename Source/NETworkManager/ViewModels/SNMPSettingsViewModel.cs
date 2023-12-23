using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Lextm.SharpSnmpLib.Messaging;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels;

public class SNMPSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    private readonly IDialogCoordinator _dialogCoordinator;

    public ICollectionView OIDProfiles { get; }

    private SNMPOIDProfileInfo _selectedOIDProfile = new();

    public SNMPOIDProfileInfo SelectedOIDProfile
    {
        get => _selectedOIDProfile;
        set
        {
            if (value == _selectedOIDProfile)
                return;

            _selectedOIDProfile = value;
            OnPropertyChanged();
        }
    }

    public List<WalkMode> WalkModes { get; private set; }

    private WalkMode _walkMode;

    public WalkMode WalkMode
    {
        get => _walkMode;
        set
        {
            if (value == _walkMode)
                return;

            if (!_isLoading)
                SettingsManager.Current.SNMP_WalkMode = value;

            _walkMode = value;
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
                SettingsManager.Current.SNMP_Timeout = value;

            _timeout = value;
            OnPropertyChanged();
        }
    }

    private int _port;

    public int Port
    {
        get => _port;
        set
        {
            if (value == _port)
                return;

            if (!_isLoading)
                SettingsManager.Current.SNMP_Port = value;

            _port = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Contructor, load settings

    public SNMPSettingsViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

        OIDProfiles = CollectionViewSource.GetDefaultView(SettingsManager.Current.SNMP_OidProfiles);
        OIDProfiles.SortDescriptions.Add(new SortDescription(nameof(SNMPOIDProfileInfo.Name),
            ListSortDirection.Ascending));

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        WalkModes = Enum.GetValues(typeof(WalkMode)).Cast<WalkMode>().OrderBy(x => x.ToString()).ToList();
        WalkMode = WalkModes.First(x => x == SettingsManager.Current.SNMP_WalkMode);
        Timeout = SettingsManager.Current.SNMP_Timeout;
        Port = SettingsManager.Current.SNMP_Port;
    }

    #endregion

    #region ICommand & Actions

    public ICommand AddOIDProfileCommand => new RelayCommand(_ => AddOIDProfileAction());

    private void AddOIDProfileAction()
    {
        AddOIDProfile().ConfigureAwait(false);
    }

    public ICommand EditOIDProfileCommand => new RelayCommand(_ => EditOIDProfileAction());

    private void EditOIDProfileAction()
    {
        EditOIDProfile().ConfigureAwait(false);
    }

    public ICommand DeleteOIDProfileCommand => new RelayCommand(_ => DeleteOIDProfileAction());

    private void DeleteOIDProfileAction()
    {
        DeleteOIDProfile().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    private async Task AddOIDProfile()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.AddOIDProfile
        };

        var viewModel = new SNMPOIDProfileViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            SettingsManager.Current.SNMP_OidProfiles.Add(new SNMPOIDProfileInfo(instance.Name, instance.OID,
                instance.Mode));
        }, async _ => { await _dialogCoordinator.HideMetroDialogAsync(this, customDialog); });

        customDialog.Content = new SNMPOIDProfileDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    public async Task EditOIDProfile()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.EditOIDProfile
        };

        var viewModel = new SNMPOIDProfileViewModel(async instance =>
        {
            await _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            SettingsManager.Current.SNMP_OidProfiles.Remove(SelectedOIDProfile);
            SettingsManager.Current.SNMP_OidProfiles.Add(new SNMPOIDProfileInfo(instance.Name, instance.OID,
                instance.Mode));
        }, async _ => { await _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, true, SelectedOIDProfile);

        customDialog.Content = new SNMPOIDProfileDialog
        {
            DataContext = viewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    private async Task DeleteOIDProfile()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.DeleteOIDProfile
        };

        var confirmDeleteViewModel = new ConfirmDeleteViewModel(_ =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.SNMP_OidProfiles.Remove(SelectedOIDProfile);
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
            Strings.DeleteOIDProfileMessage);

        customDialog.Content = new ConfirmDeleteDialog
        {
            DataContext = confirmDeleteViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    #endregion
}